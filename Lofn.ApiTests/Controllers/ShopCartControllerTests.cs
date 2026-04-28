using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.ApiTests.Helpers;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class ShopCartControllerTests
    {
        private readonly ApiTestFixture _fixture;

        public ShopCartControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var payload = TestDataHelper.CreateShopCartInfo();

            var response = await _fixture.CreateAnonymousRequest("shopcart/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Insert_WithAuth_ShouldNotReturn401()
        {
            var payload = TestDataHelper.CreateShopCartInfo();

            var response = await _fixture.CreateAuthenticatedRequest("shopcart/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().NotBe(401);
        }
    }
}
