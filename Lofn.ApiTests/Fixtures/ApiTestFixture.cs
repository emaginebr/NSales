using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;

namespace Lofn.ApiTests.Fixtures
{
    public class ApiTestFixture : IAsyncLifetime
    {
        public string BaseUrl { get; private set; } = string.Empty;
        public string AuthToken { get; private set; } = string.Empty;
        public bool IsMarketplaceTenant { get; private set; }

        private IConfiguration _configuration = null!;
        private string _tenant = string.Empty;
        private string _userAgent = "Lofn.ApiTests/1.0";
        private string _deviceFingerprint = "api-test-device";

        private string? _cachedStoreSlug;
        private readonly SemaphoreSlim _storeSlugLock = new(1, 1);

        private long? _cachedCategoryId;
        private readonly SemaphoreSlim _categoryIdLock = new(1, 1);

        public async Task InitializeAsync()
        {
            FlurlHttp.Clients.WithDefaults(builder =>
                builder.ConfigureInnerHandler(handler =>
                    handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true));

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            BaseUrl = RequireConfig("ApiBaseUrl");
            _tenant = RequireConfig("Auth:Tenant");
            _userAgent = _configuration["Auth:UserAgent"] ?? _userAgent;
            _deviceFingerprint = _configuration["Auth:DeviceFingerprint"] ?? _deviceFingerprint;

            var marketplaceRaw = _configuration["TestData:Marketplace"];
            IsMarketplaceTenant = bool.TryParse(marketplaceRaw, out var parsed) && parsed;

            var overrideToken = _configuration["Auth:TokenOverride"];
            if (!string.IsNullOrWhiteSpace(overrideToken)
                && !overrideToken.StartsWith("REPLACE_VIA_ENV_"))
            {
                AuthToken = overrideToken;
                return;
            }

            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            var authBaseUrl = RequireConfig("Auth:BaseUrl");
            var email = RequireConfig("Auth:Email");
            var password = RequireConfig("Auth:Password");
            var loginEndpoint = _configuration["Auth:LoginEndpoint"] ?? "/user/loginWithEmail";

            string responseBody;
            try
            {
                responseBody = await new Url(authBaseUrl)
                    .AppendPathSegment(loginEndpoint)
                    .WithHeader("X-Tenant-Id", _tenant)
                    .WithHeader("User-Agent", _userAgent)
                    .WithHeader("X-Device-Fingerprint", _deviceFingerprint)
                    .PostJsonAsync(new { email, password })
                    .ReceiveString();
            }
            catch (FlurlHttpException ex)
            {
                var body = await ex.GetResponseStringAsync();
                throw new Exception(
                    $"Failed to authenticate for API tests. Status: {ex.StatusCode}. " +
                    $"URL: {authBaseUrl}{loginEndpoint}. Response: {Truncate(body, 500)}. " +
                    $"Ensure NAuth is reachable and credentials in appsettings.Test.json are correct.",
                    ex);
            }

            AuthToken = ExtractToken(responseBody);
            if (string.IsNullOrWhiteSpace(AuthToken))
            {
                throw new Exception(
                    $"Login at {authBaseUrl}{loginEndpoint} returned no token. " +
                    $"Response body: {Truncate(responseBody, 500)}");
            }
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public async Task<string> GetTestStoreSlugAsync()
        {
            if (_cachedStoreSlug is not null) return _cachedStoreSlug;

            await _storeSlugLock.WaitAsync();
            try
            {
                if (_cachedStoreSlug is not null) return _cachedStoreSlug;

                var query = "{ stores(skip: 0, take: 1) { items { slug } } }";
                string responseBody;
                try
                {
                    responseBody = await new Url(BaseUrl)
                        .AppendPathSegment("graphql")
                        .WithHeader("X-Tenant-Id", _tenant)
                        .WithHeader("User-Agent", _userAgent)
                        .WithHeader("X-Device-Fingerprint", _deviceFingerprint)
                        .PostJsonAsync(new { query })
                        .ReceiveString();
                }
                catch (FlurlHttpException ex)
                {
                    var body = await ex.GetResponseStringAsync();
                    throw new Exception(
                        $"Failed to fetch a test store from {BaseUrl}/graphql. Status: {ex.StatusCode}. " +
                        $"Response: {Truncate(body, 500)}",
                        ex);
                }

                var slug = ExtractFirstStoreSlug(responseBody);
                if (string.IsNullOrWhiteSpace(slug))
                {
                    throw new Exception(
                        "No store found via public GraphQL `stores` query. Seed at least one active store " +
                        $"in the target environment ({BaseUrl}) before running these tests. " +
                        $"Response: {Truncate(responseBody, 500)}");
                }

                _cachedStoreSlug = slug;
                return slug;
            }
            finally
            {
                _storeSlugLock.Release();
            }
        }

        public async Task<long> GetTestCategoryIdAsync()
        {
            if (_cachedCategoryId is not null) return _cachedCategoryId.Value;

            await _categoryIdLock.WaitAsync();
            try
            {
                if (_cachedCategoryId is not null) return _cachedCategoryId.Value;

                _cachedCategoryId = IsMarketplaceTenant
                    ? await GetOrCreateGlobalCategoryAsync()
                    : await GetOrCreateStoreScopedCategoryAsync();
                return _cachedCategoryId.Value;
            }
            finally
            {
                _categoryIdLock.Release();
            }
        }

        private async Task<long> GetOrCreateGlobalCategoryAsync()
        {
            var listBody = await CreateAuthenticatedRequest("category-global/list")
                .AllowAnyHttpStatus()
                .GetAsync()
                .ReceiveString();
            var existing = ExtractFirstCategoryId(listBody);
            if (existing.HasValue) return existing.Value;

            var insertBody = await CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"Test Global Category {Guid.NewGuid():N}" })
                .ReceiveString();
            var created = ExtractCategoryIdFromObject(insertBody)
                ?? throw new Exception($"Failed to create a global category. Response: {Truncate(insertBody, 500)}");
            return created;
        }

        /// <summary>
        /// Seeds a parent + child pair through whichever surface is currently open
        /// (store-scoped if non-marketplace, global if marketplace) and returns
        /// (parentId, childId). Mirrors <c>SeedCategoryThroughOpenPathAsync</c> from
        /// <c>CategoryMutualExclusionTests</c> but creates two nested levels.
        /// </summary>
        public async Task<(long parentId, long childId)> SeedParentChildPairAsync(string storeSlug)
        {
            // Try store-scoped first; fall back to global (the closed surface returns 4xx and we ignore the body)
            var parentId = await TrySeedRootAsync(storeSlug);
            var childId = await TrySeedChildAsync(storeSlug, parentId);
            return (parentId, childId);
        }

        private async Task<long> TrySeedRootAsync(string storeSlug)
        {
            var storeBody = await CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"ParentRoot {Guid.NewGuid():N}" })
                .ReceiveString();

            if (TryReadCategoryId(storeBody, out var storeId)) return storeId;

            var globalBody = await CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"ParentRoot {Guid.NewGuid():N}" })
                .ReceiveString();

            if (TryReadCategoryId(globalBody, out var globalId)) return globalId;

            throw new InvalidOperationException(
                "SeedParentChildPairAsync could not seed a parent through either surface. " +
                $"store={Truncate(storeBody, 200)} global={Truncate(globalBody, 200)}");
        }

        private async Task<long> TrySeedChildAsync(string storeSlug, long parentId)
        {
            var storeBody = await CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"Child {Guid.NewGuid():N}", parentCategoryId = parentId })
                .ReceiveString();

            if (TryReadCategoryId(storeBody, out var storeId)) return storeId;

            var globalBody = await CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"Child {Guid.NewGuid():N}", parentCategoryId = parentId })
                .ReceiveString();

            if (TryReadCategoryId(globalBody, out var globalId)) return globalId;

            throw new InvalidOperationException(
                "SeedParentChildPairAsync could not seed a child through either surface. " +
                $"store={Truncate(storeBody, 200)} global={Truncate(globalBody, 200)}");
        }

        private static bool TryReadCategoryId(string body, out long categoryId)
        {
            categoryId = 0;
            if (string.IsNullOrWhiteSpace(body)) return false;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return false;
                if (doc.RootElement.TryGetProperty("categoryId", out var prop)
                    && prop.TryGetInt64(out var value))
                {
                    categoryId = value;
                    return true;
                }
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private async Task<long> GetOrCreateStoreScopedCategoryAsync()
        {
            var storeSlug = await GetTestStoreSlugAsync();

            const string query = "{ myCategories(skip: 0, take: 1) { items { categoryId storeId } } }";
            var graphBody = await CreateAuthenticatedRequest("graphql/admin")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { query })
                .ReceiveString();
            var existing = ExtractCategoryIdFromAdminQuery(graphBody);
            if (existing.HasValue) return existing.Value;

            var insertBody = await CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"Test Category {Guid.NewGuid():N}" })
                .ReceiveString();
            var created = ExtractCategoryIdFromObject(insertBody)
                ?? throw new Exception($"Failed to create a store-scoped category for store '{storeSlug}'. Response: {Truncate(insertBody, 500)}");
            return created;
        }

        private static long? ExtractFirstCategoryId(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) return null;
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("categoryId", out var prop) && prop.TryGetInt64(out var value))
                        return value;
                }
                return null;
            }
            catch (JsonException) { return null; }
        }

        private static long? ExtractCategoryIdFromObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
                if (doc.RootElement.TryGetProperty("categoryId", out var prop) && prop.TryGetInt64(out var value))
                    return value;
                return null;
            }
            catch (JsonException) { return null; }
        }

        private static long? ExtractCategoryIdFromAdminQuery(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("data", out var data)) return null;
                if (!data.TryGetProperty("myCategories", out var myCategories)) return null;
                if (!myCategories.TryGetProperty("items", out var items)
                    || items.ValueKind != JsonValueKind.Array) return null;
                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("categoryId", out var prop) && prop.TryGetInt64(out var value))
                        return value;
                }
                return null;
            }
            catch (JsonException) { return null; }
        }

        public async Task<long> SeedProductTypeAsync(string name)
        {
            var response = await CreateAuthenticatedRequest("producttype/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name });

            var body = await response.GetStringAsync();

            if ((int)response.StatusCode >= 400)
            {
                throw new Exception(
                    $"SeedProductTypeAsync('{name}') failed with HTTP {(int)response.StatusCode}. " +
                    $"The endpoint /producttype/insert requires [TenantAdmin] (IsAdmin claim). " +
                    $"Verify that the test user configured in appsettings.Test.json has IsAdmin = true. " +
                    $"Response: {Truncate(body, 500)}");
            }

            if (!TryReadIdProperty(body, "productTypeId", out var id))
            {
                throw new Exception($"SeedProductTypeAsync('{name}') returned non-JSON or missing productTypeId. Response: {Truncate(body, 500)}");
            }
            return id;
        }

        public async Task<long> SeedProductTypeFilterAsync(long productTypeId, string label, string dataType, string[]? allowedValues = null)
        {
            var payload = allowedValues is null
                ? (object)new { label, dataType, isRequired = false, displayOrder = 0 }
                : new { label, dataType, isRequired = false, displayOrder = 0, allowedValues };

            var response = await CreateAuthenticatedRequest($"producttype/{productTypeId}/filter/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            var body = await response.GetStringAsync();

            if ((int)response.StatusCode >= 400)
            {
                throw new Exception(
                    $"SeedProductTypeFilterAsync('{label}') failed with HTTP {(int)response.StatusCode}. " +
                    $"Response: {Truncate(body, 500)}");
            }

            if (!TryReadIdProperty(body, "filterId", out var id))
            {
                throw new Exception($"SeedProductTypeFilterAsync('{label}') returned non-JSON or missing filterId. Response: {Truncate(body, 500)}");
            }
            return id;
        }

        private static bool TryReadIdProperty(string body, string propertyName, out long id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(body)) return false;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return false;
                if (!doc.RootElement.TryGetProperty(propertyName, out var prop)) return false;
                if (!prop.TryGetInt64(out var value)) return false;
                id = value;
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public async Task<long> SeedCustomizationGroupAsync(
            long productTypeId,
            string label,
            string selectionMode = "single",
            bool isRequired = false,
            int displayOrder = 0)
        {
            var response = await CreateAuthenticatedRequest($"producttype/{productTypeId}/customization/group/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { label, selectionMode, isRequired, displayOrder });

            var body = await response.GetStringAsync();

            if ((int)response.StatusCode >= 400)
            {
                throw new Exception(
                    $"SeedCustomizationGroupAsync('{label}') failed with HTTP {(int)response.StatusCode}. " +
                    $"Response: {Truncate(body, 500)}");
            }

            if (!TryReadIdProperty(body, "groupId", out var id))
            {
                throw new Exception($"SeedCustomizationGroupAsync('{label}') returned non-JSON or missing groupId. Response: {Truncate(body, 500)}");
            }
            return id;
        }

        public async Task<long> SeedCustomizationOptionAsync(
            long groupId,
            string label,
            long priceDeltaCents = 0,
            bool isDefault = false,
            int displayOrder = 0)
        {
            var response = await CreateAuthenticatedRequest($"producttype/customization/group/{groupId}/option/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { label, priceDeltaCents, isDefault, displayOrder });

            var body = await response.GetStringAsync();

            if ((int)response.StatusCode >= 400)
            {
                throw new Exception(
                    $"SeedCustomizationOptionAsync('{label}') failed with HTTP {(int)response.StatusCode}. " +
                    $"Response: {Truncate(body, 500)}");
            }

            if (!TryReadIdProperty(body, "optionId", out var id))
            {
                throw new Exception($"SeedCustomizationOptionAsync('{label}') returned non-JSON or missing optionId. Response: {Truncate(body, 500)}");
            }
            return id;
        }

        public async Task LinkCategoryToProductTypeAsync(long categoryId, long productTypeId)
        {
            var path = IsMarketplaceTenant
                ? $"category-global/{categoryId}/producttype/{productTypeId}"
                : $"category/{categoryId}/producttype/{productTypeId}";

            var response = await CreateAuthenticatedRequest(path)
                .AllowAnyHttpStatus()
                .PutAsync(null);

            if ((int)response.StatusCode >= 400)
            {
                var body = await response.GetStringAsync();
                throw new Exception($"LinkCategoryToProductTypeAsync failed with {response.StatusCode}. Body: {Truncate(body, 500)}");
            }
        }

        public IFlurlRequest CreateAuthenticatedRequest(string path) =>
            new Url(BaseUrl).AppendPathSegment(path)
                .WithHeader("X-Tenant-Id", _tenant)
                .WithHeader("User-Agent", _userAgent)
                .WithHeader("X-Device-Fingerprint", _deviceFingerprint)
                .WithOAuthBearerToken(AuthToken);

        public IFlurlRequest CreateAnonymousRequest(string path) =>
            new Url(BaseUrl).AppendPathSegment(path)
                .WithHeader("X-Tenant-Id", _tenant)
                .WithHeader("User-Agent", _userAgent)
                .WithHeader("X-Device-Fingerprint", _deviceFingerprint);

        private string RequireConfig(string key)
        {
            var value = _configuration[key]
                ?? throw new Exception($"Missing required config key '{key}'.");

            if (value.StartsWith("REPLACE_VIA_ENV_"))
            {
                var envVar = value.Substring("REPLACE_VIA_ENV_".Length);
                throw new Exception(
                    $"Config key '{key}' still holds the placeholder. " +
                    $"Export environment variable '{envVar}' (or fill it in appsettings.Test.json) before running the tests.");
            }
            return value;
        }

        private static string ExtractToken(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return string.Empty;

                foreach (var name in new[] { "token", "Token", "accessToken", "AccessToken", "jwt", "Jwt" })
                {
                    if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                        return prop.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                {
                    foreach (var name in new[] { "token", "Token", "accessToken", "AccessToken" })
                    {
                        if (data.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                            return prop.GetString() ?? string.Empty;
                    }
                }

                return string.Empty;
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }

        private static string ExtractFirstStoreSlug(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("data", out var data)
                    || data.ValueKind != JsonValueKind.Object) return string.Empty;
                if (!data.TryGetProperty("stores", out var stores)
                    || stores.ValueKind != JsonValueKind.Object) return string.Empty;
                if (!stores.TryGetProperty("items", out var items)
                    || items.ValueKind != JsonValueKind.Array) return string.Empty;

                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("slug", out var slug) && slug.ValueKind == JsonValueKind.String)
                    {
                        var value = slug.GetString();
                        if (!string.IsNullOrWhiteSpace(value)) return value;
                    }
                }
                return string.Empty;
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }

        private static string Truncate(string? value, int max) =>
            string.IsNullOrEmpty(value) ? string.Empty :
            value.Length <= max ? value : value.Substring(0, max) + "...";
    }
}
