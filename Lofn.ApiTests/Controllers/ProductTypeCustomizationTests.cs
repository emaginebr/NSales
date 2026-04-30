using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using System.Text.Json;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class ProductTypeCustomizationTests
    {
        private readonly ApiTestFixture _fixture;

        public ProductTypeCustomizationTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task InsertGroup_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("producttype/1/customization/group/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { label = "Anon", selectionMode = "single", isRequired = false, displayOrder = 0 });

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InsertOption_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("producttype/customization/group/1/option/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { label = "Anon", priceDeltaCents = 0, isDefault = false, displayOrder = 0 });

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InsertGroup_WithThreeOptions_ShouldPersistTreeAndReturnViaGet()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"Equipamento {Guid.NewGuid():N}");
            var groupId = await _fixture.SeedCustomizationGroupAsync(typeId, "Processador", "single", isRequired: true);

            var optI3 = await _fixture.SeedCustomizationOptionAsync(groupId, "i3", priceDeltaCents: 0, isDefault: true);
            var optI5 = await _fixture.SeedCustomizationOptionAsync(groupId, "i5", priceDeltaCents: 50000);
            var optI7 = await _fixture.SeedCustomizationOptionAsync(groupId, "i7", priceDeltaCents: 90000);

            optI3.Should().BeGreaterThan(0);
            optI5.Should().BeGreaterThan(0);
            optI7.Should().BeGreaterThan(0);

            var typeBody = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync()
                .ReceiveString();

            using var doc = JsonDocument.Parse(typeBody);
            doc.RootElement.TryGetProperty("customizationGroups", out var groups).Should().BeTrue();
            groups.ValueKind.Should().Be(JsonValueKind.Array);

            var group = groups.EnumerateArray()
                .FirstOrDefault(g => g.TryGetProperty("groupId", out var p) && p.GetInt64() == groupId);
            group.ValueKind.Should().Be(JsonValueKind.Object);

            group.GetProperty("label").GetString().Should().Be("Processador");
            group.GetProperty("selectionMode").GetString().Should().Be("single");
            group.GetProperty("isRequired").GetBoolean().Should().BeTrue();

            group.TryGetProperty("options", out var options).Should().BeTrue();
            var optionIds = options.EnumerateArray()
                .Select(o => o.GetProperty("optionId").GetInt64())
                .ToList();
            optionIds.Should().Contain(new[] { optI3, optI5, optI7 });

            var defaults = options.EnumerateArray()
                .Where(o => o.GetProperty("isDefault").GetBoolean())
                .Select(o => o.GetProperty("optionId").GetInt64())
                .ToList();
            defaults.Should().ContainSingle().Which.Should().Be(optI3);
        }

        [Fact]
        public async Task InsertOption_SecondDefaultInSingleGroup_ShouldReturn400Or422()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"DupDefault {Guid.NewGuid():N}");
            var groupId = await _fixture.SeedCustomizationGroupAsync(typeId, "Tamanho", "single");
            await _fixture.SeedCustomizationOptionAsync(groupId, "P", priceDeltaCents: 0, isDefault: true);

            var response = await _fixture.CreateAuthenticatedRequest($"producttype/customization/group/{groupId}/option/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new { label = "M", priceDeltaCents = 1000, isDefault = true, displayOrder = 1 });

            ((int)response.StatusCode).Should().BeOneOf(400, 422);
        }

        [Fact]
        public async Task UpdateGroup_MultiToSingleWithTwoDefaults_ShouldReturn400Or422()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"Multi2Single {Guid.NewGuid():N}");
            var groupId = await _fixture.SeedCustomizationGroupAsync(typeId, "Adicionais", "multi");
            await _fixture.SeedCustomizationOptionAsync(groupId, "Bacon", priceDeltaCents: 500, isDefault: true);
            await _fixture.SeedCustomizationOptionAsync(groupId, "Queijo", priceDeltaCents: 300, isDefault: true);

            var response = await _fixture.CreateAuthenticatedRequest("producttype/customization/group/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new
                {
                    groupId,
                    label = "Adicionais",
                    selectionMode = "single",
                    isRequired = false,
                    displayOrder = 0
                });

            ((int)response.StatusCode).Should().BeOneOf(400, 422);
        }

        [Fact]
        public async Task UpdateGroup_SingleToMulti_ShouldReturn200()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"Single2Multi {Guid.NewGuid():N}");
            var groupId = await _fixture.SeedCustomizationGroupAsync(typeId, "Cor", "single");

            var response = await _fixture.CreateAuthenticatedRequest("producttype/customization/group/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new
                {
                    groupId,
                    label = "Cores",
                    selectionMode = "multi",
                    isRequired = true,
                    displayOrder = 2
                });

            response.StatusCode.Should().Be(200);

            var typeBody = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync()
                .ReceiveString();

            using var doc = JsonDocument.Parse(typeBody);
            var group = doc.RootElement.GetProperty("customizationGroups").EnumerateArray()
                .First(g => g.GetProperty("groupId").GetInt64() == groupId);
            group.GetProperty("selectionMode").GetString().Should().Be("multi");
            group.GetProperty("label").GetString().Should().Be("Cores");
            group.GetProperty("isRequired").GetBoolean().Should().BeTrue();
        }

        [Fact]
        public async Task UpdateOption_ShouldReflectInGetTypeAsync()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"OptUpdate {Guid.NewGuid():N}");
            var groupId = await _fixture.SeedCustomizationGroupAsync(typeId, "Memória", "single");
            var optionId = await _fixture.SeedCustomizationOptionAsync(groupId, "8GB", priceDeltaCents: 0);

            var response = await _fixture.CreateAuthenticatedRequest("producttype/customization/option/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new
                {
                    optionId,
                    label = "16GB",
                    priceDeltaCents = 40000,
                    isDefault = false,
                    displayOrder = 0
                });

            response.StatusCode.Should().Be(200);

            var typeBody = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync()
                .ReceiveString();

            using var doc = JsonDocument.Parse(typeBody);
            var option = doc.RootElement.GetProperty("customizationGroups").EnumerateArray()
                .First(g => g.GetProperty("groupId").GetInt64() == groupId)
                .GetProperty("options").EnumerateArray()
                .First(o => o.GetProperty("optionId").GetInt64() == optionId);
            option.GetProperty("label").GetString().Should().Be("16GB");
            option.GetProperty("priceDeltaCents").GetInt64().Should().Be(40000);
        }

        [Fact]
        public async Task DeleteGroup_ShouldCascadeOptionsFromTree()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"DelGroup {Guid.NewGuid():N}");
            var groupId = await _fixture.SeedCustomizationGroupAsync(typeId, "Adicionais", "multi");
            await _fixture.SeedCustomizationOptionAsync(groupId, "Alface");
            await _fixture.SeedCustomizationOptionAsync(groupId, "Tomate");

            var deleteResponse = await _fixture.CreateAuthenticatedRequest($"producttype/customization/group/delete/{groupId}")
                .AllowAnyHttpStatus()
                .DeleteAsync();
            deleteResponse.StatusCode.Should().Be(204);

            var typeBody = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync()
                .ReceiveString();

            using var doc = JsonDocument.Parse(typeBody);
            var groups = doc.RootElement.GetProperty("customizationGroups").EnumerateArray()
                .Select(g => g.GetProperty("groupId").GetInt64())
                .ToList();
            groups.Should().NotContain(groupId);
        }

        [Fact]
        public async Task DeleteOption_ShouldRemoveFromGroupOptions()
        {
            var typeId = await _fixture.SeedProductTypeAsync($"DelOpt {Guid.NewGuid():N}");
            var groupId = await _fixture.SeedCustomizationGroupAsync(typeId, "Cor", "single");
            var optionId = await _fixture.SeedCustomizationOptionAsync(groupId, "Vermelho");

            var deleteResponse = await _fixture.CreateAuthenticatedRequest($"producttype/customization/option/delete/{optionId}")
                .AllowAnyHttpStatus()
                .DeleteAsync();
            deleteResponse.StatusCode.Should().Be(204);

            var typeBody = await _fixture.CreateAuthenticatedRequest($"producttype/{typeId}")
                .AllowAnyHttpStatus()
                .GetAsync()
                .ReceiveString();

            using var doc = JsonDocument.Parse(typeBody);
            var options = doc.RootElement.GetProperty("customizationGroups").EnumerateArray()
                .First(g => g.GetProperty("groupId").GetInt64() == groupId)
                .GetProperty("options").EnumerateArray()
                .Select(o => o.GetProperty("optionId").GetInt64())
                .ToList();
            options.Should().NotContain(optionId);
        }
    }
}
