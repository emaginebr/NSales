using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;
using Lofn.DTO.Product;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class ImageControllerTests
    {
        private readonly ApiTestFixture _fixture;

        public ImageControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Upload_WithoutAuth_ShouldReturn401()
        {
            using var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(TestDataHelper.CreateMinimalPngBytes()), "file", "image.png" }
            };

            var response = await _fixture.CreateAnonymousRequest("image/upload")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .PostAsync(content);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task List_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("image/list")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task List_WithAuth_ShouldNotReturn401()
        {
            var response = await _fixture.CreateAuthenticatedRequest("image/list")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Delete_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("image/delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Delete_WithAuth_ShouldNotReturn401()
        {
            var response = await _fixture.CreateAuthenticatedRequest("image/delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Upload_WithValidImage_ShouldReturnImageWithIdAndUrl()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var product = await CreateProductAsync(storeSlug);

            var uploaded = await UploadImageAsync(product!.ProductId);

            uploaded.Should().NotBeNull();
            uploaded!.ImageId.Should().BeGreaterThan(0);
            uploaded.ProductId.Should().Be(product.ProductId);
            uploaded.Image.Should().NotBeNullOrWhiteSpace();
            uploaded.ImageUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task List_AfterUpload_ShouldContainUploadedImage()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var product = await CreateProductAsync(storeSlug);
            var uploaded = await UploadImageAsync(product!.ProductId);

            var images = await _fixture.CreateAuthenticatedRequest("image/list")
                .AppendPathSegment(product.ProductId)
                .GetJsonAsync<IList<ProductImageInfo>>();

            images.Should().NotBeNull();
            images.Should().Contain(i => i.ImageId == uploaded!.ImageId);
        }

        [Fact]
        public async Task Delete_AfterUpload_ShouldRemoveImageFromList()
        {
            var storeSlug = await _fixture.GetTestStoreSlugAsync();
            var product = await CreateProductAsync(storeSlug);
            var uploaded = await UploadImageAsync(product!.ProductId);

            var deleteResponse = await _fixture.CreateAuthenticatedRequest("image/delete")
                .AppendPathSegment(uploaded!.ImageId)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            deleteResponse.StatusCode.Should().Be(204);

            var images = await _fixture.CreateAuthenticatedRequest("image/list")
                .AppendPathSegment(product.ProductId)
                .GetJsonAsync<IList<ProductImageInfo>>();

            images.Should().NotContain(i => i.ImageId == uploaded.ImageId);
        }

        private async Task<ProductInfo?> CreateProductAsync(string storeSlug)
        {
            var payload = TestDataHelper.CreateProductInsertInfo();
            payload.CategoryId = await _fixture.GetTestCategoryIdAsync();

            return await _fixture.CreateAuthenticatedRequest("product")
                .AppendPathSegment(storeSlug)
                .AppendPathSegment("insert")
                .PostJsonAsync(payload)
                .ReceiveJson<ProductInfo>();
        }

        private async Task<ProductImageInfo?> UploadImageAsync(long productId, int sortOrder = 0)
        {
            using var content = new MultipartFormDataContent
            {
                {
                    new ByteArrayContent(TestDataHelper.CreateMinimalPngBytes())
                    {
                        Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png") }
                    },
                    "file",
                    $"test-{Guid.NewGuid():N}.png"
                }
            };

            return await _fixture.CreateAuthenticatedRequest("image/upload")
                .AppendPathSegment(productId)
                .SetQueryParam("sortOrder", sortOrder)
                .PostAsync(content)
                .ReceiveJson<ProductImageInfo>();
        }
    }
}
