using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class StoreControllerTests
    {
        private readonly ApiTestFixture _fixture;

        public StoreControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateStoreInsertInfo();

            var response = await _fixture.CreateAnonymousRequest("store/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Insert_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateStoreInsertInfo();

            var response = await _fixture.CreateAuthenticatedRequest("store/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Update_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateStoreUpdateInfo();

            var response = await _fixture.CreateAnonymousRequest("store/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Update_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateStoreUpdateInfo();

            var response = await _fixture.CreateAuthenticatedRequest("store/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Delete_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("store/delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Delete_WithAuth_ShouldNotReturn401()
        {
            var response = await _fixture.CreateAuthenticatedRequest("store/delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task UploadLogo_WithoutAuth_ShouldReturn401()
        {
            using var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 }), "file", "logo.png" }
            };

            var response = await _fixture.CreateAnonymousRequest("store/uploadLogo")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .PostAsync(content);

            response.StatusCode.Should().Be(401);
        }
    }
}
