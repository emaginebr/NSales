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
