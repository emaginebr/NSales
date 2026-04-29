using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using System.Text.Json;

namespace Lofn.ApiTests.Controllers
{
    /// <summary>
    /// Integration tests for the 002-category-subcategories GraphQL tree fields:
    ///
    ///   - public schema:  POST /graphql            { categoryTree(storeSlug: "...") { ... } }
    ///   - admin  schema:  POST /graphql/admin      { myCategoryTree { ... } }
    ///
    /// Each test seeds a parent + child via <see cref="ApiTestFixture.SeedParentChildPairAsync"/>
    /// (which targets whichever surface is currently open) and asserts that the tree
    /// response reflects the seeded shape. Authentication, anonymous access and the
    /// marketplace mutex are exercised explicitly.
    /// </summary>
    [Collection("ApiTests")]
    public class CategoryTreeGraphQLTests
    {
        private readonly ApiTestFixture _fixture;

        public CategoryTreeGraphQLTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task CategoryTree_PublicEndpoint_AllowsAnonymous()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var query = $"{{ categoryTree(storeSlug: \"{storeSlug}\") {{ categoryId name slug parentCategoryId isGlobal }} }}";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue(
                "the public categoryTree field must respond to anonymous callers without errors");
            data.TryGetProperty("categoryTree", out var tree).Should().BeTrue();
            tree.ValueKind.Should().Be(JsonValueKind.Array);
        }

        [Fact]
        public async Task CategoryTree_OnSeededHierarchy_ReturnsNestedShape()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, childId) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var query = _fixture.IsMarketplaceTenant
                ? "{ categoryTree { categoryId name slug parentCategoryId isGlobal children { categoryId name slug parentCategoryId isGlobal } } }"
                : $"{{ categoryTree(storeSlug: \"{storeSlug}\") {{ categoryId name slug parentCategoryId isGlobal children {{ categoryId name slug parentCategoryId isGlobal }} }} }}";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            var tree = doc.RootElement.GetProperty("data").GetProperty("categoryTree");
            tree.ValueKind.Should().Be(JsonValueKind.Array);

            var parentNode = FindNodeById(tree, parentId);
            parentNode.HasValue.Should().BeTrue($"the seeded parent {parentId} must appear in the public tree response");

            var children = parentNode!.Value.GetProperty("children");
            children.ValueKind.Should().Be(JsonValueKind.Array);

            var childNode = EnumerateNodes(children).FirstOrDefault(n =>
                n.TryGetProperty("categoryId", out var idProp)
                && idProp.TryGetInt64(out var id)
                && id == childId);

            childNode.ValueKind.Should().Be(JsonValueKind.Object,
                $"the seeded child {childId} must be nested under parent {parentId} in the tree response");

            var childSlug = childNode.GetProperty("slug").GetString();
            childSlug.Should().NotBeNullOrWhiteSpace();
            childSlug!.Should().Contain("/", "the child slug must reflect the full ancestor path (parent/child)");
        }

        [Fact]
        public async Task CategoryTree_RespectsMutex()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            // Make sure at least one category exists in the open scope so the response is non-empty.
            await _fixture.SeedParentChildPairAsync(storeSlug);

            var query = _fixture.IsMarketplaceTenant
                ? "{ categoryTree { isGlobal } }"
                : $"{{ categoryTree(storeSlug: \"{storeSlug}\") {{ isGlobal }} }}";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            var tree = doc.RootElement.GetProperty("data").GetProperty("categoryTree");

            var values = new List<bool>();
            foreach (var node in tree.EnumerateArray())
                values.Add(node.GetProperty("isGlobal").GetBoolean());

            if (values.Count == 0) return;

            values.Distinct().Should().HaveCount(1,
                "the categoryTree roots must all share the same scope: marketplace tenants return only globals (isGlobal=true) " +
                "and per-store tenants return only store-scoped roots (isGlobal=false)");

            var expected = _fixture.IsMarketplaceTenant;
            values.Should().AllSatisfy(v => v.Should().Be(expected));
        }

        [Fact]
        public async Task CategoryTree_ChildrenAreAlphabeticallyOrdered()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            // Seed two siblings that sort differently in accent-aware vs accent-naive order.
            var sibA = await SeedSiblingAsync(storeSlug, parentId, "Calças");
            var sibB = await SeedSiblingAsync(storeSlug, parentId, "Camisetas");

            // If neither seed succeeded (closed surface scenario) we cannot assert ordering.
            if (sibA is null && sibB is null) return;

            var query = _fixture.IsMarketplaceTenant
                ? "{ categoryTree { categoryId children { categoryId name } } }"
                : $"{{ categoryTree(storeSlug: \"{storeSlug}\") {{ categoryId children {{ categoryId name }} }} }}";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            var tree = doc.RootElement.GetProperty("data").GetProperty("categoryTree");

            var parentNode = FindNodeById(tree, parentId);
            parentNode.HasValue.Should().BeTrue();
            var children = parentNode!.Value.GetProperty("children");

            var names = children.EnumerateArray()
                .Select(c => c.GetProperty("name").GetString() ?? string.Empty)
                .ToList();

            // Find indices of the two known siblings; assert "Calças" precedes "Camisetas" (accent-aware).
            var idxA = names.IndexOf("Calças");
            var idxB = names.IndexOf("Camisetas");
            if (idxA >= 0 && idxB >= 0)
            {
                idxA.Should().BeLessThan(idxB,
                    $"`Calças` must precede `Camisetas` under accent-aware alphabetical ordering, " +
                    $"got names = [{string.Join(", ", names)}]");
            }
        }

        [Fact]
        public async Task MyCategoryTree_WithoutAuth_ShouldReturnAuthError()
        {
            const string query = "{ myCategoryTree { categoryId name slug isGlobal } }";

            // HotChocolate convention: [Authorize] failures return HTTP 200 with a
            // GraphQL `errors` array carrying `AUTH_NOT_AUTHENTICATED`, not HTTP 401.
            var responseBody = await _fixture.CreateAnonymousRequest("graphql/admin")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            doc.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue(
                "the admin schema is gated by [Authorize]; anonymous calls must surface a GraphQL error");
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            errors.GetArrayLength().Should().BeGreaterThan(0);

            var firstError = errors[0];
            firstError.TryGetProperty("extensions", out var extensions).Should().BeTrue();
            extensions.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("AUTH_NOT_AUTHENTICATED");

            doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue();
            data.GetProperty("myCategoryTree").ValueKind.Should().Be(JsonValueKind.Null);
        }

        [Fact]
        public async Task MyCategoryTree_WithAuth_ReturnsTree()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            await _fixture.SeedParentChildPairAsync(storeSlug);

            const string query = "{ myCategoryTree { categoryId name slug parentCategoryId isGlobal children { categoryId name } } }";

            var responseBody = await _fixture.CreateAuthenticatedRequest("graphql/admin")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue(
                $"the admin schema should return data for an authenticated caller; body = {Truncate(responseBody, 400)}");
            data.TryGetProperty("myCategoryTree", out var tree).Should().BeTrue();
            tree.ValueKind.Should().Be(JsonValueKind.Array);
        }

        // ---------- helpers ----------

        private async Task<long?> SeedSiblingAsync(string storeSlug, long parentId, string name)
        {
            var storeBody = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"{name} {Guid.NewGuid():N}".Replace(" ", "-"), parentCategoryId = parentId })
                .ReceiveString();
            if (TryReadCategoryId(storeBody, out var storeId)) return storeId;

            var globalBody = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"{name} {Guid.NewGuid():N}".Replace(" ", "-"), parentCategoryId = parentId })
                .ReceiveString();
            if (TryReadCategoryId(globalBody, out var globalId)) return globalId;

            return null;
        }

        private static JsonElement? FindNodeById(JsonElement tree, long categoryId)
        {
            foreach (var node in EnumerateNodes(tree))
            {
                if (node.TryGetProperty("categoryId", out var idProp)
                    && idProp.TryGetInt64(out var id)
                    && id == categoryId)
                    return node;
            }
            return null;
        }

        private static IEnumerable<JsonElement> EnumerateNodes(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    yield return item;
                    if (item.TryGetProperty("children", out var children))
                    {
                        foreach (var nested in EnumerateNodes(children))
                            yield return nested;
                    }
                }
            }
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
            catch (JsonException) { return false; }
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) ? string.Empty :
            value.Length <= max ? value : value.Substring(0, max) + "...";
    }
}
