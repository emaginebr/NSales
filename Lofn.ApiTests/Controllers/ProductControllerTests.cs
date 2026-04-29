using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;
using Lofn.DTO.Product;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class ProductControllerTests
    {
        private const string DefaultStoreSlug = "test-store";

        private readonly ApiTestFixture _fixture;

        public ProductControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateProductInsertInfo();

            var response = await _fixture.CreateAnonymousRequest("product")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Insert_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateProductInsertInfo();

            var response = await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Update_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateProductUpdateInfo();

            var response = await _fixture.CreateAnonymousRequest("product")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Update_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateProductUpdateInfo();

            var response = await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Search_AsAnonymous_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateProductSearchParam();

            var response = await _fixture.CreateAnonymousRequest("product/search")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Search_AsAnonymous_ShouldReturnOk()
        {
            var payload = TestDataHelper.CreateProductSearchParam();

            var response = await _fixture.CreateAnonymousRequest("product/search")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Insert_WithValidPayload_ShouldReturnCreatedProductWithId()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var categoryId = await _fixture.GetTestCategoryIdAsync();
            var payload = TestDataHelper.CreateProductInsertInfo();
            payload.CategoryId = categoryId;

            var created = await InsertProductAsync(storeSlug, payload);

            created.Should().NotBeNull();
            created!.ProductId.Should().BeGreaterThan(0);
            created.Name.Should().Be(payload.Name);
            created.Description.Should().Be(payload.Description);
            created.Price.Should().Be(payload.Price);
            created.Status.Should().Be(payload.Status);
            created.ProductType.Should().Be(payload.ProductType);
            created.CategoryId.Should().Be(categoryId);
        }

        [Fact]
        public async Task Update_AfterInsert_ShouldReflectChanges()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var categoryId = await _fixture.GetTestCategoryIdAsync();

            var insertPayload = TestDataHelper.CreateProductInsertInfo();
            insertPayload.CategoryId = categoryId;
            var inserted = await InsertProductAsync(storeSlug, insertPayload);

            var updatePayload = TestDataHelper.CreateProductUpdateInfo(inserted!.ProductId);
            updatePayload.CategoryId = categoryId;
            updatePayload.Price = 199.90;
            updatePayload.Discount = 15;

            var updated = await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("update")
                .PostJsonAsync(updatePayload)
                .ReceiveJson<ProductInfo>();

            updated.Should().NotBeNull();
            updated.ProductId.Should().Be(inserted.ProductId);
            updated.Name.Should().Be(updatePayload.Name);
            updated.Price.Should().Be(updatePayload.Price);
            updated.Discount.Should().Be(updatePayload.Discount);
            updated.CategoryId.Should().Be(categoryId);
        }

        [Fact]
        public async Task Update_WithInactiveStatus_ShouldSoftDeleteProduct()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var categoryId = await _fixture.GetTestCategoryIdAsync();

            var insertPayload = TestDataHelper.CreateProductInsertInfo();
            insertPayload.CategoryId = categoryId;
            var inserted = await InsertProductAsync(storeSlug, insertPayload);

            var deletePayload = TestDataHelper.CreateProductUpdateInfo(inserted!.ProductId, inserted.Name);
            deletePayload.CategoryId = categoryId;
            deletePayload.Price = inserted.Price;
            deletePayload.Status = ProductStatusEnum.Inactive;

            var afterDelete = await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("update")
                .PostJsonAsync(deletePayload)
                .ReceiveJson<ProductInfo>();

            afterDelete.Should().NotBeNull();
            afterDelete.ProductId.Should().Be(inserted.ProductId);
            afterDelete.Status.Should().Be(ProductStatusEnum.Inactive);
        }

        [Fact]
        public async Task Insert_OnMarketplaceTenant_WithLegacyCategory_ShouldReturn400()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var legacyCategoryId = await FindLegacyCategoryIdAsync(storeSlug);
            if (legacyCategoryId is null) return;

            var payload = TestDataHelper.CreateProductInsertInfo();
            payload.CategoryId = legacyCategoryId;

            var response = await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Insert_OnNonMarketplaceTenant_WithSameStoreCategory_ShouldReturn200()
        {
            if (_fixture.IsMarketplaceTenant) return;

            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var categoryId = await _fixture.GetTestCategoryIdAsync();
            var payload = TestDataHelper.CreateProductInsertInfo();
            payload.CategoryId = categoryId;

            var response = await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(200);
        }

        private async Task<long?> FindLegacyCategoryIdAsync(string storeSlug)
        {
            var query = $"{{ stores(where: {{ slug: {{ eq: \"{storeSlug}\" }} }}) {{ items {{ products {{ category {{ categoryId storeId }} }} }} }} }}";
            try
            {
                var response = await _fixture.CreateAnonymousRequest("graphql")
                    .PostJsonAsync(new { query })
                    .ReceiveString();
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                if (!doc.RootElement.TryGetProperty("data", out var data)) return null;
                if (!data.TryGetProperty("stores", out var stores)) return null;
                if (!stores.TryGetProperty("items", out var items) || items.GetArrayLength() == 0) return null;
                foreach (var product in items[0].GetProperty("products").EnumerateArray())
                {
                    if (!product.TryGetProperty("category", out var category)
                        || category.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                    if (category.TryGetProperty("storeId", out var sid)
                        && sid.ValueKind == System.Text.Json.JsonValueKind.Number
                        && category.TryGetProperty("categoryId", out var cid))
                    {
                        return cid.GetInt64();
                    }
                }
            }
            catch
            {
                // best-effort lookup; if it fails, the test silently no-ops
            }
            return null;
        }

        private async Task<ProductInfo?> InsertProductAsync(string storeSlug, ProductInsertInfo payload) =>
            await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .PostJsonAsync(payload)
                .ReceiveJson<ProductInfo>();
    }
}
