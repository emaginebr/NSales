using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;
using Lofn.DTO.Category;

namespace Lofn.ApiTests.Controllers
{
    /// <summary>
    /// Verifies the marketplace mode enforces mutual exclusion between the two
    /// category management surfaces. Exactly one of the following should succeed
    /// for any given tenant configuration; the other should return 403.
    ///
    ///   - Store-scoped:   POST /category/{slug}/insert       (works only when Marketplace = false)
    ///   - Tenant-global:  POST /category-global/insert       (works only when Marketplace = true)
    /// </summary>
    [Collection("ApiTests")]
    public class CategoryMutualExclusionTests
    {
        private readonly ApiTestFixture _fixture;

        public CategoryMutualExclusionTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task ExactlyOneCategoryPath_ShouldBeOpen_PerTenantMode()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();

            var storeScopedResponse = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryInsertInfo());

            var globalResponse = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CategoryGlobalInsertInfo
                {
                    Name = $"Mutex {Guid.NewGuid():N}"
                });

            var storeAccepted = storeScopedResponse.StatusCode != 403;
            var globalAccepted = globalResponse.StatusCode != 403;

            (storeAccepted ^ globalAccepted).Should().BeTrue(
                "exactly one of the two category-management surfaces must be open for a given tenant " +
                $"(Marketplace = {_fixture.IsMarketplaceTenant}); store-scoped returned {storeScopedResponse.StatusCode}, " +
                $"global returned {globalResponse.StatusCode}");

            if (_fixture.IsMarketplaceTenant)
            {
                storeScopedResponse.StatusCode.Should().Be(403, "store-scoped surface must be locked when Marketplace = true");
                globalResponse.StatusCode.Should().NotBe(403, "global surface must be open when Marketplace = true and caller is admin");
            }
            else
            {
                storeScopedResponse.StatusCode.Should().NotBe(403, "store-scoped surface must be open when Marketplace = false");
                globalResponse.StatusCode.Should().Be(403, "global surface must be locked when Marketplace = false");
            }
        }

        [Fact]
        public async Task ExactlyOneCategoryPath_ShouldBeOpen_OnUpdate()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();

            var storeScopedResponse = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryUpdateInfo());

            var globalResponse = await _fixture.CreateAuthenticatedRequest("category-global/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CategoryGlobalUpdateInfo
                {
                    CategoryId = 999_999,
                    Name = $"Mutex {Guid.NewGuid():N}"
                });

            if (_fixture.IsMarketplaceTenant)
            {
                storeScopedResponse.StatusCode.Should().Be(403);
                globalResponse.StatusCode.Should().NotBe(403);
            }
            else
            {
                storeScopedResponse.StatusCode.Should().NotBe(403);
                globalResponse.StatusCode.Should().Be(403);
            }
        }

        [Fact]
        public async Task ExactlyOneCategoryPath_ShouldBeOpen_OnDelete()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();

            var storeScopedResponse = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(999_999)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            var globalResponse = await _fixture.CreateAuthenticatedRequest("category-global/delete/999999")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            if (_fixture.IsMarketplaceTenant)
            {
                storeScopedResponse.StatusCode.Should().Be(403);
                globalResponse.StatusCode.Should().NotBe(403);
            }
            else
            {
                storeScopedResponse.StatusCode.Should().NotBe(403);
                globalResponse.StatusCode.Should().Be(403);
            }
        }
    }
}
