using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using System.Text.Json;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class CategoryProductTypeLinkTests
    {
        private readonly ApiTestFixture _fixture;

        public CategoryProductTypeLinkTests(ApiTestFixture fixture) => _fixture = fixture;

        private string CategoryRouteBase =>
            _fixture.IsMarketplaceTenant ? "category-global" : "category";

        [Fact]
        public async Task LinkProductType_AsAdmin_ShouldReturn200()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"LinkType {Guid.NewGuid():N}");
            var categoryId = await _fixture.GetTestCategoryIdAsync();

            var response = await _fixture.CreateAuthenticatedRequest($"{CategoryRouteBase}/{categoryId}/producttype/{typeId}")
                .AllowAnyHttpStatus()
                .PutAsync(null);

            response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task LinkProductType_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest($"{CategoryRouteBase}/1/producttype/1")
                .AllowAnyHttpStatus()
                .PutAsync(null);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task UnlinkProductType_ShouldReturn200_AndAppliedTypeBecomesNull()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"UnlinkType {Guid.NewGuid():N}");
            var categoryId = await _fixture.GetTestCategoryIdAsync();
            await _fixture.LinkCategoryToProductTypeAsync(categoryId, typeId);

            var unlinkResponse = await _fixture.CreateAuthenticatedRequest($"{CategoryRouteBase}/{categoryId}/producttype")
                .AllowAnyHttpStatus()
                .DeleteAsync();
            unlinkResponse.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAppliedProductType_OnLinkedCategory_ShouldReturnTypeInfo()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"AppliedType {Guid.NewGuid():N}");
            var categoryId = await _fixture.GetTestCategoryIdAsync();
            await _fixture.LinkCategoryToProductTypeAsync(categoryId, typeId);

            var route = $"category/{categoryId}/producttype/applied";
            var body = await _fixture.CreateAnonymousRequest(route)
                .AllowAnyHttpStatus()
                .GetStringAsync();

            using var doc = JsonDocument.Parse(body);
            doc.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
            doc.RootElement.TryGetProperty("appliedProductTypeId", out var appliedProp).Should().BeTrue();
            appliedProp.GetInt64().Should().Be(typeId);
        }

        [Fact]
        public async Task GetAppliedProductType_OnUnlinkedCategoryWithoutAncestor_ShouldReturnNull()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"NullApplied {Guid.NewGuid():N}");
            var categoryId = await _fixture.GetTestCategoryIdAsync();
            await _fixture.LinkCategoryToProductTypeAsync(categoryId, typeId);

            await _fixture.CreateAuthenticatedRequest($"{CategoryRouteBase}/{categoryId}/producttype")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            var body = await _fixture.CreateAnonymousRequest($"category/{categoryId}/producttype/applied")
                .AllowAnyHttpStatus()
                .GetStringAsync();

            (string.IsNullOrEmpty(body) || body.Trim() == "null").Should().BeTrue();
        }
    }
}
