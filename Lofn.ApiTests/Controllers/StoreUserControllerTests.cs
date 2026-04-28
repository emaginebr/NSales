using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class StoreUserControllerTests
    {
        private const string DefaultStoreSlug = "test-store";

        private readonly ApiTestFixture _fixture;

        public StoreUserControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task List_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("storeuser")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("list")
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task List_WithAuth_ShouldNotReturn401()
        {
            var response = await _fixture.CreateAuthenticatedRequest("storeuser")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("list")
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateStoreUserInsertInfo();

            var response = await _fixture.CreateAnonymousRequest("storeuser")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Insert_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateStoreUserInsertInfo();

            var response = await _fixture.CreateAuthenticatedRequest("storeuser")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Delete_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("storeuser")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Delete_WithAuth_ShouldNotReturn401()
        {
            var response = await _fixture.CreateAuthenticatedRequest("storeuser")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().NotBe(401);
        }
    }
}
