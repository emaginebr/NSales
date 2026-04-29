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
    /// Anonymous-auth tests for the legacy store-scoped category surface.
    /// Behavioural tests (insert/update/delete success across modes) live in
    /// <see cref="CategoryMutualExclusionTests"/>, which exercises both surfaces
    /// and asserts the marketplace mutex.
    /// </summary>
    [Collection("ApiTests")]
    public class CategoryControllerTests
    {
        private const string DefaultStoreSlug = "test-store";

        private readonly ApiTestFixture _fixture;

        public CategoryControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateCategoryInsertInfo();

            var response = await _fixture.CreateAnonymousRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Update_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateCategoryUpdateInfo();

            var response = await _fixture.CreateAnonymousRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Delete_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(401);
        }

        // -------------------------------------------------------------------
        // 002-category-subcategories: parent-aware behaviour (T037).
        //
        // Each test exercises the store-scoped surface specifically.
        // - When the tenant is non-marketplace, the surface is OPEN and
        //   parent-aware behaviour is asserted end-to-end.
        // - When the tenant is marketplace, the surface is closed by the
        //   `Marketplace=false` gate and the API returns 403; the test
        //   asserts that the gate is enforced.
        // -------------------------------------------------------------------

        [Fact]
        public async Task InsertWithParent_OnStoreScopedSurface_RespectsMarketplaceGate()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryInsertInfo(parentCategoryId: parentId));

            if (_fixture.IsMarketplaceTenant)
                response.StatusCode.Should().Be(403, "store-scoped surface is closed when Marketplace=true");
            else
                response.StatusCode.Should().BeInRange(200, 299,
                    "store-scoped insert with a valid parent must succeed when Marketplace=false");
        }

        [Fact]
        public async Task InsertWithParent_NonExistentParent_ReturnsValidationError()
        {
            if (_fixture.IsMarketplaceTenant) return; // surface closed; covered by gate test above
            var storeSlug = await _fixture.GetTestStoreSlugAsync();

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryInsertInfo(parentCategoryId: 9_999_999_999));

            response.StatusCode.Should().BeInRange(400, 499,
                "the API must reject a non-existent ParentCategoryId with a 4xx and a descriptive message");
            var body = await response.GetStringAsync();
            body.Should().ContainAny("Parent", "parent", "not found");
        }

        [Fact]
        public async Task InsertWithParent_SiblingNameCollision_ReturnsValidationError()
        {
            if (_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var sharedName = $"Sibling {Guid.NewGuid():N}";
            var first = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryInsertInfo(name: sharedName, parentCategoryId: parentId));
            first.StatusCode.Should().BeInRange(200, 299, "first sibling-insert must succeed to set up the collision");

            var collision = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryInsertInfo(name: sharedName, parentCategoryId: parentId));

            collision.StatusCode.Should().BeInRange(400, 499);
            var body = await collision.GetStringAsync();
            body.Should().ContainAny("already exists", "exists under this parent");
        }

        [Fact]
        public async Task UpdateWithCycle_OnStoreScopedSurface_ReturnsValidationError()
        {
            if (_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, childId) = await _fixture.SeedParentChildPairAsync(storeSlug);

            // Try to set the parent's parent to its own child → cycle.
            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryUpdateInfo(categoryId: parentId, parentCategoryId: childId));

            response.StatusCode.Should().BeInRange(400, 499);
            var body = await response.GetStringAsync();
            body.Should().ContainAny("cycle", "Cycle");
        }

        [Fact]
        public async Task DeleteWithChildren_OnStoreScopedSurface_ReturnsValidationError()
        {
            if (_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(parentId)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().BeInRange(400, 499,
                "the API must refuse to delete a category that has subcategories");
            var body = await response.ResponseMessage.Content.ReadAsStringAsync();
            body.Should().ContainAny("subcategories", "children", "remove them first");
        }

        // -------------------------------------------------------------------
        // 002-category-subcategories US3: slug cascade on rename (T058).
        //
        // Renames a parent category and verifies that descendants' slugs
        // recompute to reflect the new ancestor segment.
        // Only runs when the store-scoped surface is the open one.
        // -------------------------------------------------------------------

        [Fact]
        public async Task Update_Rename_CascadesSlugToDescendants()
        {
            if (_fixture.IsMarketplaceTenant) return;
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, childId) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var renamedParentName = $"Renamed-{Guid.NewGuid():N}";
            var renameResponse = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryUpdateInfo(categoryId: parentId, name: renamedParentName));
            renameResponse.StatusCode.Should().BeInRange(200, 299,
                "the rename of the parent must succeed before we can assert the cascade");

            var renamedBody = await renameResponse.GetStringAsync();
            using var renamedDoc = JsonDocument.Parse(renamedBody);
            var newParentSlug = renamedDoc.RootElement.GetProperty("slug").GetString();
            newParentSlug.Should().NotBeNullOrWhiteSpace();

            var query = $"{{ categoryTree(storeSlug: \"{storeSlug}\") {{ categoryId slug children {{ categoryId slug }} }} }}";
            var treeBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var treeDoc = JsonDocument.Parse(treeBody);
            var tree = treeDoc.RootElement.GetProperty("data").GetProperty("categoryTree");
            var childSlug = FindChildSlug(tree, parentId, childId);
            childSlug.Should().NotBeNullOrWhiteSpace(
                $"the seeded child {childId} must still appear under parent {parentId} after the rename");
            childSlug!.Should().StartWith($"{newParentSlug}/",
                $"the cascade must rewrite the child slug to start with the renamed parent's new slug `{newParentSlug}` " +
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
