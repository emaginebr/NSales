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
    /// Verifies that category management is mutually exclusive between the two surfaces:
    ///
    ///   - Store-scoped:   POST /category/{slug}/insert | update | delete
    ///   - Tenant-global:  POST /category-global/insert | update | delete
    ///
    /// Each test exercises BOTH paths and asserts that exactly one of them is operational
    /// for the current tenant configuration. The other must reject the request — either by
    /// gate (403) for update/delete or by failing the operation for insert.
    ///
    /// Pass condition: XOR — exactly one path succeeds.
    /// Fail conditions: both succeed (mutex broken) OR both fail (no path open).
    /// </summary>
    [Collection("ApiTests")]
    public class CategoryMutualExclusionTests
    {
        private readonly ApiTestFixture _fixture;

        public CategoryMutualExclusionTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_ShouldSucceedOnExactlyOnePath()
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
                    Name = $"GlobalMutex {Guid.NewGuid():N}"
                });

            var storeSucceeded = IsSuccess(storeScopedResponse.StatusCode);
            var globalSucceeded = IsSuccess(globalResponse.StatusCode);

            (storeSucceeded ^ globalSucceeded).Should().BeTrue(
                $"exactly one of the two category-insert surfaces must succeed " +
                $"(store={storeScopedResponse.StatusCode}, global={globalResponse.StatusCode}); " +
                "if neither succeeds the tenant has no path to register categories, " +
                "and if both succeed the marketplace mutex is broken");
        }

        [Fact]
        public async Task Update_ShouldSucceedOnExactlyOnePath()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var seededCategoryId = await SeedCategoryThroughOpenPathAsync(storeSlug);

            var storeScopedResponse = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CategoryUpdateInfo
                {
                    CategoryId = seededCategoryId,
                    Name = $"StoreUpd {Guid.NewGuid():N}"
                });

            var globalResponse = await _fixture.CreateAuthenticatedRequest("category-global/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CategoryGlobalUpdateInfo
                {
                    CategoryId = seededCategoryId,
                    Name = $"GlobalUpd {Guid.NewGuid():N}"
                });

            var storeSucceeded = IsSuccess(storeScopedResponse.StatusCode);
            var globalSucceeded = IsSuccess(globalResponse.StatusCode);

            (storeSucceeded ^ globalSucceeded).Should().BeTrue(
                $"exactly one of the two category-update surfaces must succeed for category {seededCategoryId} " +
                $"(store={storeScopedResponse.StatusCode}, global={globalResponse.StatusCode})");
        }

        [Fact]
        public async Task InsertSubcategory_ShouldSucceedOnExactlyOnePath()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var (parentId, _) = await _fixture.SeedParentChildPairAsync(storeSlug);

            var storeScopedResponse = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryInsertInfo(parentCategoryId: parentId));

            var globalResponse = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CategoryGlobalInsertInfo
                {
                    Name = $"GlobalSubMutex {Guid.NewGuid():N}",
                    ParentCategoryId = parentId
                });

            var storeSucceeded = IsSuccess(storeScopedResponse.StatusCode);
            var globalSucceeded = IsSuccess(globalResponse.StatusCode);

            (storeSucceeded ^ globalSucceeded).Should().BeTrue(
                $"exactly one of the two subcategory-insert surfaces must succeed under parent {parentId} " +
                $"(store={storeScopedResponse.StatusCode}, global={globalResponse.StatusCode}); " +
                "if both succeed the marketplace mutex is broken even for nested categories, " +
                "and if neither succeeds the tenant has no path to register subcategories");
        }

        [Fact]
        public async Task Delete_ShouldSucceedOnExactlyOnePath()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var storeCandidate = await SeedCategoryThroughOpenPathAsync(storeSlug);
            var globalCandidate = await SeedCategoryThroughOpenPathAsync(storeSlug);

            var storeScopedResponse = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(storeCandidate)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            var globalResponse = await _fixture.CreateAuthenticatedRequest($"category-global/delete/{globalCandidate}")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            var storeSucceeded = IsSuccess(storeScopedResponse.StatusCode);
            var globalSucceeded = IsSuccess(globalResponse.StatusCode);

            (storeSucceeded ^ globalSucceeded).Should().BeTrue(
                $"exactly one of the two category-delete surfaces must succeed " +
                $"(store={storeScopedResponse.StatusCode} for id {storeCandidate}, " +
                $"global={globalResponse.StatusCode} for id {globalCandidate})");
        }

        private async Task<long> SeedCategoryThroughOpenPathAsync(string storeSlug)
        {
            var storeBody = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(TestDataHelper.CreateCategoryInsertInfo())
                .ReceiveString();

            if (TryReadCategoryId(storeBody, out var storeId)) return storeId;

            var globalBody = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CategoryGlobalInsertInfo
                {
                    Name = $"Seed {Guid.NewGuid():N}"
                })
                .ReceiveString();

            if (TryReadCategoryId(globalBody, out var globalId)) return globalId;

            throw new InvalidOperationException(
                "Failed to seed a category through either path. " +
                $"store body: {Truncate(storeBody)}, global body: {Truncate(globalBody)}");
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

        private static bool IsSuccess(int statusCode) => statusCode >= 200 && statusCode < 300;

        private static string Truncate(string value, int max = 200) =>
            value.Length <= max ? value : value.Substring(0, max) + "...";
    }
}
