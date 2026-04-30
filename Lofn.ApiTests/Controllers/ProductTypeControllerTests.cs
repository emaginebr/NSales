using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using System.Net;
using System.Text.Json;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class ProductTypeControllerTests
    {
        private readonly ApiTestFixture _fixture;

        public ProductTypeControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("producttype/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name = $"Anon {Guid.NewGuid():N}" });

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Insert_AsAdmin_ShouldReturn200_AndGetByIdReturnsTheType()
        {
            var name = $"Calçado {Guid.NewGuid():N}";
            var insertBody = await _fixture.CreateAuthenticatedRequest("producttype/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name })
                .ReceiveString();

            using var insertDoc = JsonDocument.Parse(insertBody);
            insertDoc.RootElement.TryGetProperty("productTypeId", out var typeIdProp).Should().BeTrue();
            var typeId = typeIdProp.GetInt64();
            typeId.Should().BeGreaterThan(0);

            var getResponse = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync();
            getResponse.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Update_ChangesName_ShouldReturn200()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"Roupa {Guid.NewGuid():N}");

            var newName = $"Roupa-Updated {Guid.NewGuid():N}";
            var response = await _fixture.CreateAuthenticatedRequest("producttype/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { productTypeId = typeId, name = newName });

            response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_ShouldReturn204_AndSubsequentGetReturns404()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"Equipamento {Guid.NewGuid():N}");

            var deleteResponse = await _fixture.CreateAuthenticatedRequest($"producttype/delete/{typeId}")
                .AllowAnyHttpStatus()
                .DeleteAsync();
            deleteResponse.StatusCode.Should().Be(204);

            var getResponse = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync();
            getResponse.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Insert_DuplicateName_ShouldReturn422()
        {
            var name = $"Carro {Guid.NewGuid():N}";
            await _fixture.SeedProductTypeAsync(name);

            var response = await _fixture.CreateAuthenticatedRequest("producttype/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { name });

            ((int)response.StatusCode).Should().BeOneOf(400, 422);
        }

        [Fact]
        public async Task InsertFilter_EnumWithAllowedValues_ShouldSucceed_AndGetReturnsAllowedValues()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"Comida {Guid.NewGuid():N}");
            var allowed = new[] { "Pequeno", "Médio", "Grande" };
            var filterId = await _fixture.SeedProductTypeFilterAsync(typeId, "Tamanho", "enum", allowed);

            filterId.Should().BeGreaterThan(0);

            var typeBody = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync()
                .ReceiveString();

            using var doc = JsonDocument.Parse(typeBody);
            doc.RootElement.TryGetProperty("filters", out var filters).Should().BeTrue();
            filters.ValueKind.Should().Be(JsonValueKind.Array);

            var filter = filters.EnumerateArray()
                .FirstOrDefault(f => f.TryGetProperty("filterId", out var p) && p.GetInt64() == filterId);
            filter.ValueKind.Should().Be(JsonValueKind.Object);

            filter.TryGetProperty("allowedValues", out var avProp).Should().BeTrue();
            var values = avProp.EnumerateArray().Select(v => v.GetString()).ToList();
            values.Should().BeEquivalentTo(allowed);
        }

        [Fact]
        public async Task InsertFilter_DuplicateLabel_ShouldReturn422()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"DupLabel {Guid.NewGuid():N}");
            await _fixture.SeedProductTypeFilterAsync(typeId, "Cor", "text");

            var response = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}/filter/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { label = "Cor", dataType = "text", isRequired = false, displayOrder = 0 });

            ((int)response.StatusCode).Should().BeOneOf(400, 422);
        }

        [Fact]
        public async Task InsertFilter_EnumWithoutAllowedValues_ShouldReturn422()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"EnumMissingAV {Guid.NewGuid():N}");

            var response = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}/filter/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { label = "Tamanho", dataType = "enum", isRequired = false, displayOrder = 0 });

            ((int)response.StatusCode).Should().BeOneOf(400, 422);
        }

        [Fact]
        public async Task DeleteFilter_ShouldReturn204()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"DelFilter {Guid.NewGuid():N}");
            var filterId = await _fixture.SeedProductTypeFilterAsync(typeId, "Marca", "text");

            var response = await _fixture.CreateAuthenticatedRequest($"producttype/filter/delete/{filterId}")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(204);
        }
    }
}
