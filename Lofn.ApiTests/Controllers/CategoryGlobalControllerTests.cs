using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;
using Lofn.DTO.Category;
using System.Net;
using System.Text.Json;

namespace Lofn.ApiTests.Controllers
{
    /// <summary>
    /// Anonymous-auth tests for the marketplace-only global category surface.
    /// Behavioural tests (insert/update/delete success across modes) live in
    /// <see cref="CategoryMutualExclusionTests"/>, which exercises both surfaces
    /// and asserts the marketplace mutex.
    /// </summary>
    [Collection("ApiTests")]
    public class CategoryGlobalControllerTests
    {
        private readonly ApiTestFixture _fixture;

        public CategoryGlobalControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var payload = new CategoryGlobalInsertInfo { Name = $"Anon {Guid.NewGuid():N}" };

            var response = await _fixture.CreateAnonymousRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task List_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("category-global/list")
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Update_WithoutAuth_ShouldReturn401()
        {
            var payload = new CategoryGlobalUpdateInfo { CategoryId = 1, Name = "X" };

            var response = await _fixture.CreateAnonymousRequest("category-global/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Delete_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("category-global/delete/1")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(401);
        }

        // -------------------------------------------------------------------
        // 002-category-subcategories: parent-aware behaviour (T038).
        //
        // Each test exercises the global surface specifically.
        // - When the tenant is marketplace, the surface is OPEN and
        //   parent-aware behaviour is asserted end-to-end.
        // - When the tenant is non-marketplace, the surface is closed by
        //   the [MarketplaceAdmin] gate and the API returns 403.
        // -------------------------------------------------------------------

        [Fact]
        public async Task InsertWithParent_OnGlobalSurface_RespectsMarketplaceGate()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var response = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryGlobalInsertInfo(parentCategoryId: parentId));

            if (_fixture.IsMarketplaceTenant)
                response.StatusCode.Should().BeInRange(200, 299,
                    "global insert with a valid parent must succeed when Marketplace=true");
            else
                response.StatusCode.Should().Be(403, "global surface is closed when Marketplace=false");
        }

        [Fact]
        public async Task InsertWithParent_NonExistentParent_ReturnsValidationError()
        {
            if (!_fixture.IsMarketplaceTenant) return; // surface closed; covered by gate test above

            var response = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryGlobalInsertInfo(parentCategoryId: 9_999_999_999));

            response.StatusCode.Should().BeInRange(400, 499,
                "the API must reject a non-existent ParentCategoryId on the global surface");
            var body = await response.GetStringAsync();
            body.Should().ContainAny("Parent", "parent", "not found");
        }

        [Fact]
        public async Task InsertWithParent_SiblingNameCollision_ReturnsValidationError()
        {
            if (!_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var sharedName = $"GlobalSibling {Guid.NewGuid():N}";
            var first = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryGlobalInsertInfo(name: sharedName, parentCategoryId: parentId));
            first.StatusCode.Should().BeInRange(200, 299, "first sibling-insert must succeed to set up the collision");

            var collision = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryGlobalInsertInfo(name: sharedName, parentCategoryId: parentId));

            collision.StatusCode.Should().BeInRange(400, 499);
            var body = await collision.GetStringAsync();
            body.Should().ContainAny("already exists", "exists under this parent");
        }

        [Fact]
        public async Task UpdateWithCycle_OnGlobalSurface_ReturnsValidationError()
        {
            if (!_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, childId) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var response = await _fixture.CreateAuthenticatedRequest("category-global/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryGlobalUpdateInfo(categoryId: parentId, parentCategoryId: childId));

            response.StatusCode.Should().BeInRange(400, 499);
            var body = await response.GetStringAsync();
            body.Should().ContainAny("cycle", "Cycle");
        }

        [Fact]
        public async Task DeleteWithChildren_OnGlobalSurface_ReturnsValidationError()
        {
            if (!_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var response = await _fixture.CreateAuthenticatedRequest($"category-global/delete/{parentId}")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().BeInRange(400, 499,
                "the API must refuse to delete a global category that has subcategories");
            var body = await response.ResponseMessage.Content.ReadAsStringAsync();
            body.Should().ContainAny("subcategories", "children", "remove them first");
        }

        // -------------------------------------------------------------------
        // 002-category-subcategories US3: slug cascade on rename (T059).
        //
        // Renames a global parent category and verifies that descendants'
        // slugs recompute to reflect the new ancestor segment. Only runs
        // when the global surface is the open one (Marketplace=true).
        // -------------------------------------------------------------------

        [Fact]
        public async Task Update_Rename_CascadesSlugToDescendants()
        {
            if (!_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, childId) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var renamedParentName = $"GlobalRenamed-{Guid.NewGuid():N}";
            var renameResponse = await _fixture.CreateAuthenticatedRequest("category-global/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryGlobalUpdateInfo(categoryId: parentId, name: renamedParentName));
            renameResponse.StatusCode.Should().BeInRange(200, 299,
                "the rename of the global parent must succeed before we can assert the cascade");

            var renamedBody = await renameResponse.GetStringAsync();
            using var renamedDoc = JsonDocument.Parse(renamedBody);
            var newParentSlug = renamedDoc.RootElement.GetProperty("slug").GetString();
            newParentSlug.Should().NotBeNullOrWhiteSpace();

            // Marketplace tenants expose the global tree via the public schema with no storeSlug argument.
            const string query = "{ categoryTree { categoryId slug children { categoryId slug } } }";
            var treeBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var treeDoc = JsonDocument.Parse(treeBody);
            var tree = treeDoc.RootElement.GetProperty("data").GetProperty("categoryTree");
            var childSlug = FindChildSlug(tree, parentId, childId);
            childSlug.Should().NotBeNullOrWhiteSpace(
                $"the seeded global child {childId} must still appear under parent {parentId} after the rename");
            childSlug!.Should().StartWith($"{newParentSlug}/",
                $"the cascade must rewrite the global child slug to start with the renamed parent's new slug `{newParentSlug}` " +
                $"(actual = `{childSlug}`)");
        }

        private static string? FindChildSlug(JsonElement tree, long parentId, long childId)
        {
            foreach (var node in EnumerateNodes(tree))
            {
                if (!node.TryGetProperty("categoryId", out var idProp)
                    || !idProp.TryGetInt64(out var id)
                    || id != parentId)
                    continue;

                if (!node.TryGetProperty("children", out var children)
                    || children.ValueKind != JsonValueKind.Array)
                    return null;

                foreach (var child in children.EnumerateArray())
                {
                    if (child.TryGetProperty("categoryId", out var cIdProp)
                        && cIdProp.TryGetInt64(out var cId)
                        && cId == childId)
                        return child.TryGetProperty("slug", out var slug) ? slug.GetString() : null;
                }
                return null;
            }
            return null;
        }

        private static IEnumerable<JsonElement> EnumerateNodes(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array) yield break;
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
}
