using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class HealthCheckTests
    {
        private readonly ApiTestFixture _fixture;

        public HealthCheckTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Root_AsAnonymous_ShouldReturnOk()
        {
            var response = await _fixture.CreateAnonymousRequest("/")
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().Be(200);
        }
    }
}
