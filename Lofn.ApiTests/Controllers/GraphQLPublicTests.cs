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

        /// <summary>
        /// Marketplace-mode invariant: a single tenant exposes either globals only
        /// (Marketplace = true) or store-scoped only (Marketplace = false). Mixing
        /// scopes in the same response would mean the resolver branch is broken.
        /// </summary>
        [Fact]
        public async Task Categories_AllItems_ShouldHaveConsistentIsGlobalValue()
        {
            const string query = "{ categories(skip:0, take:50) { items { categoryId isGlobal } } }";

            var responseBody = await _fixture.CreateAnonymousRequest("graphql")
                .PostJsonAsync(new { query })
                .ReceiveString();

            using var doc = JsonDocument.Parse(responseBody);
            var items = doc.RootElement.GetProperty("data").GetProperty("categories").GetProperty("items");

            var values = new List<bool>();
            foreach (var item in items.EnumerateArray())
            {
                values.Add(item.GetProperty("isGlobal").GetBoolean());
            }

            if (values.Count == 0) return;

            values.Distinct().Should().HaveCount(1,
                "every category returned by the public `categories` query must share the same `isGlobal` value " +
                $"(found a mix: {string.Join(",", values.Distinct())}); the resolver should branch coherently on tenant Marketplace mode");
        }
    }
}
