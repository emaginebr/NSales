using FluentAssertions;
using Flurl.Http;
using Lofn.ApiTests.Fixtures;
using Lofn.DTO.Category;

namespace Lofn.ApiTests.Controllers
{
    [Collection("ApiTests")]
    public class CategoryGlobalControllerTests
    {
        private readonly ApiTestFixture _fixture;

        public CategoryGlobalControllerTests(ApiTestFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Insert_WithoutAuth_ShouldReturn401()
        {
            var payload = new CategoryGlobalInsertInfo { Name = $"Anon {Guid.NewGuid():N}" };

            var response = await _fixture.CreateAnonymousRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task List_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("category-global/list")
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Update_WithoutAuth_ShouldReturn401()
        {
            var payload = new CategoryGlobalUpdateInfo { CategoryId = 1, Name = "X" };

            var response = await _fixture.CreateAnonymousRequest("category-global/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Delete_WithoutAuth_ShouldReturn401()
        {
            var response = await _fixture.CreateAnonymousRequest("category-global/delete/1")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Insert_OnNonMarketplaceTenant_ShouldReturn403()
        {
            if (_fixture.IsMarketplaceTenant) return;

            var payload = new CategoryGlobalInsertInfo { Name = $"NoMarket {Guid.NewGuid():N}" };

            var response = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .AllowAnyHttpStatus()
                .PostJsonAsync(payload);

            response.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task List_OnNonMarketplaceTenant_ShouldReturn403()
        {
            if (_fixture.IsMarketplaceTenant) return;

            var response = await _fixture.CreateAuthenticatedRequest("category-global/list")
                .AllowAnyHttpStatus()
                .GetAsync();

            response.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task Insert_OnMarketplaceTenant_AsAdmin_ShouldReturn200()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var payload = new CategoryGlobalInsertInfo { Name = $"Eletrônicos {Guid.NewGuid():N}" };

            var info = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .PostJsonAsync(payload)
                .ReceiveJson<CategoryInfo>();

            info.Should().NotBeNull();
            info!.CategoryId.Should().BeGreaterThan(0);
            info.Name.Should().Be(payload.Name);
            info.StoreId.Should().BeNull();
            info.IsGlobal.Should().BeTrue();
            info.Slug.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task List_OnMarketplaceTenant_AsAdmin_ShouldContainInsertedCategory()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var insertPayload = new CategoryGlobalInsertInfo { Name = $"Listed {Guid.NewGuid():N}" };
            var inserted = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .PostJsonAsync(insertPayload)
                .ReceiveJson<CategoryInfo>();

            var list = await _fixture.CreateAuthenticatedRequest("category-global/list")
                .GetAsync()
                .ReceiveJson<List<CategoryInfo>>();

            list.Should().NotBeNull();
            list.Should().Contain(c => c.CategoryId == inserted!.CategoryId);
            list.Should().OnlyContain(c => c.IsGlobal && c.StoreId == null);
        }

        [Fact]
        public async Task Update_OnMarketplaceTenant_ShouldReflectChange()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var inserted = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .PostJsonAsync(new CategoryGlobalInsertInfo { Name = $"Old {Guid.NewGuid():N}" })
                .ReceiveJson<CategoryInfo>();

            var newName = $"Updated {Guid.NewGuid():N}";
            var updated = await _fixture.CreateAuthenticatedRequest("category-global/update")
                .PostJsonAsync(new CategoryGlobalUpdateInfo { CategoryId = inserted!.CategoryId, Name = newName })
                .ReceiveJson<CategoryInfo>();

            updated.Should().NotBeNull();
            updated!.CategoryId.Should().Be(inserted.CategoryId);
            updated.Name.Should().Be(newName);
            updated.StoreId.Should().BeNull();
            updated.IsGlobal.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_OnMarketplaceTenant_ShouldRemoveCategory()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var inserted = await _fixture.CreateAuthenticatedRequest("category-global/insert")
                .PostJsonAsync(new CategoryGlobalInsertInfo { Name = $"ToDelete {Guid.NewGuid():N}" })
                .ReceiveJson<CategoryInfo>();

            var deleteResponse = await _fixture.CreateAuthenticatedRequest($"category-global/delete/{inserted!.CategoryId}")
                .AllowAnyHttpStatus()
                .DeleteAsync();

            deleteResponse.StatusCode.Should().Be(204);

            var listAfter = await _fixture.CreateAuthenticatedRequest("category-global/list")
                .GetAsync()
                .ReceiveJson<List<CategoryInfo>>();

            listAfter.Should().NotContain(c => c.CategoryId == inserted.CategoryId);
        }

        [Fact]
        public async Task Update_WithInvalidName_ShouldReturn400()
        {
            if (!_fixture.IsMarketplaceTenant) return;

            var response = await _fixture.CreateAuthenticatedRequest("category-global/update")
                .AllowAnyHttpStatus()
                .PostJsonAsync(new CategoryGlobalUpdateInfo { CategoryId = 999_999, Name = string.Empty });

            response.StatusCode.Should().Be(400);
        }
    }
}
