using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using System.Text.Json;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class GraphQLPublicTests
    {
        private readonly ApiTestFixture _fixture;

        public GraphQLPublicTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Categories_ShouldExposeIsGlobalField()
        {
            const string query = "{ categories(skip:0, take:5) { items { categoryId name slug isGlobal } } }";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue("response should not contain GraphQL errors");
            data.TryGetProperty("categories", out var categories).Should().BeTrue();
            categories.TryGetProperty("items", out var items).Should().BeTrue();
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }

        [Fact]
        public async Task Categories_OnMarketplaceTenant_ShouldReturnOnlyGlobals()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            const string query = "{ categories(skip:0, take:50) { items { categoryId isGlobal } } }";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            var items = doc.RootElement.GetProperty("data").GetProperty("categories").GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
                item.GetProperty("isGlobal").GetBoolean().Should().BeTrue();
            }
        }

        [Fact]
        public async Task Categories_OnNonMarketplaceTenant_ShouldReturnNonGlobal()
        {
            if (_fixture.IsMarketplaceTenant) return;

            const string query = "{ categories(skip:0, take:50) { items { categoryId isGlobal } } }";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            var items = doc.RootElement.GetProperty("data").GetProperty("categories").GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
                item.GetProperty("isGlobal").GetBoolean().Should().BeFalse();
            }
        }
    }
}
