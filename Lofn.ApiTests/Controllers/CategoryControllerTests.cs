using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;

namespace Lofn.ApiTests.Controllers
{
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
        public async Task Insert_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateCategoryInsertInfo();

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
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
        public async Task Update_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateCategoryUpdateInfo();

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
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

        [Fact]
        public async Task Delete_WithAuth_ShouldNotReturn401()
        {
            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().NotBe(401);
        }

        [Fact]
        public async Task Insert_OnMarketplaceTenant_ShouldReturn403()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var payload = TestDataHelper.CreateCategoryInsertInfo();

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task Update_OnMarketplaceTenant_ShouldReturn403()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var payload = TestDataHelper.CreateCategoryUpdateInfo();

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task Delete_OnMarketplaceTenant_ShouldReturn403()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("delete")
                .AppendPathSegment(1)
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task Insert_OnNonMarketplaceTenant_ShouldNotReturn403()
        {
            if (_fixture.IsMarketplaceTenant) return;

            var payload = TestDataHelper.CreateCategoryInsertInfo();

            var response = await _fixture.CreateAuthenticatedRequest("category")
                .AppendPathSegment(DefaultStoreSlug)
                .AppendPathSegment("insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(403);
        }
    }
}
