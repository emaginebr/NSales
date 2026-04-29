using FluentValidation;
using FluentValidation.Results;
using Lofn.Domain.Core;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Lofn.DTO.Category;
using Lofn.Infra.Interfaces.Repository;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lofn.Tests.Domain.Services
{
    public class CategoryServiceMarketplaceTest
    {
        private readonly Mock<ISlugGenerator> _slugGeneratorMock = new();
        private readonly Mock<ICategoryRepository<CategoryModel>> _categoryRepositoryMock = new();
        private readonly Mock<IStoreRepository<StoreModel>> _storeRepositoryMock = new();
        private readonly Mock<IValidator<CategoryInsertInfo>> _insertValidatorMock = new();
        private readonly Mock<IValidator<CategoryUpdateInfo>> _updateValidatorMock = new();
        private readonly Mock<IValidator<CategoryGlobalInsertInfo>> _globalInsertValidatorMock = new();
        private readonly Mock<IValidator<CategoryGlobalUpdateInfo>> _globalUpdateValidatorMock = new();
        private readonly CategoryService _sut;

        public CategoryServiceMarketplaceTest()
        {
            _insertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryInsertInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _updateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryUpdateInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _globalInsertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalInsertInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _globalUpdateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalUpdateInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _sut = new CategoryService(
                _slugGeneratorMock.Object,
                _categoryRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _insertValidatorMock.Object,
                _updateValidatorMock.Object,
                _globalInsertValidatorMock.Object,
                _globalUpdateValidatorMock.Object
            );
        }

        [Fact]
        public async Task InsertGlobalAsync_ShouldCreateCategoryWithStoreIdNull()
        {
            var info = new CategoryGlobalInsertInfo { Name = "Periféricos" };
            _slugGeneratorMock.Setup(x => x.Generate("Periféricos")).Returns("perifericos");
            _categoryRepositoryMock.Setup(x => x.ExistSlugInTenantAsync(null, "perifericos")).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync((CategoryModel m) =>
                {
                    m.CategoryId = 42;
                    return m;
                });

            var result = await _sut.InsertGlobalAsync(info);

            Assert.Equal(42, result.CategoryId);
            Assert.Equal("Periféricos", result.Name);
            Assert.Equal("perifericos", result.Slug);
            Assert.Null(result.StoreId);
            Assert.True(result.IsGlobal);
        }

        [Fact]
        public async Task InsertGlobalAsync_ShouldAutoSuffix_WhenSlugConflicts()
        {
            var info = new CategoryGlobalInsertInfo { Name = "Cat" };
            _slugGeneratorMock.Setup(x => x.Generate("Cat")).Returns("cat");
            _categoryRepositoryMock.SetupSequence(x => x.ExistSlugInTenantAsync(null, It.IsAny<string>()))
                .ReturnsAsync(true)    // "cat" taken
                .ReturnsAsync(false);  // "cat-1" free (kebab-case suffix)
            _categoryRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync((CategoryModel m) => m);

            var result = await _sut.InsertGlobalAsync(info);

            Assert.Equal("cat-1", result.Slug);
        }

        [Fact]
        public async Task UpdateGlobalAsync_ShouldThrow_WhenCategoryIsStoreScoped()
        {
            var info = new CategoryGlobalUpdateInfo { CategoryId = 5, Name = "X" };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(new CategoryModel { CategoryId = 5, StoreId = 10, Name = "Loja-A" });

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateGlobalAsync(info));
            Assert.Contains("not global", ex.Message);
        }

        [Fact]
        public async Task UpdateGlobalAsync_ShouldThrow_WhenCategoryNotFound()
        {
            var info = new CategoryGlobalUpdateInfo { CategoryId = 99, Name = "X" };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((CategoryModel)null);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateGlobalAsync(info));
            Assert.Contains("not found", ex.Message);
        }

        [Fact]
        public async Task UpdateGlobalAsync_ShouldUpdateNameAndSlug_WhenCategoryIsGlobal()
        {
            var info = new CategoryGlobalUpdateInfo { CategoryId = 5, Name = "Periféricos & Acessórios" };
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(new CategoryModel { CategoryId = 5, StoreId = null, Name = "Old", Slug = "old" });
            _slugGeneratorMock.Setup(x => x.Generate("Periféricos & Acessórios"))
                .Returns("perifericos-acessorios");
            _categoryRepositoryMock.Setup(x => x.ExistSlugInTenantAsync(5, "perifericos-acessorios"))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync((CategoryModel m) => m);

            var result = await _sut.UpdateGlobalAsync(info);

            Assert.Equal("Periféricos & Acessórios", result.Name);
            Assert.Equal("perifericos-acessorios", result.Slug);
            Assert.Null(result.StoreId);
        }

        [Fact]
        public async Task DeleteGlobalAsync_ShouldThrow_WhenCategoryIsStoreScoped()
        {
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(new CategoryModel { CategoryId = 5, StoreId = 10 });

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.DeleteGlobalAsync(5));
            Assert.Contains("not global", ex.Message);
        }

        [Fact]
        public async Task DeleteGlobalAsync_ShouldThrow_WhenCategoryNotFound()
        {
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((CategoryModel)null);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.DeleteGlobalAsync(99));
            Assert.Contains("not found", ex.Message);
        }

        [Fact]
        public async Task DeleteGlobalAsync_ShouldDelete_WhenCategoryIsGlobal()
        {
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(new CategoryModel { CategoryId = 5, StoreId = null });

            await _sut.DeleteGlobalAsync(5);

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(5), Times.Once);
        }

        [Fact]
        public async Task ListGlobalAsync_ShouldReturnOnlyGlobals()
        {
            var rows = new[]
            {
                new CategoryModel { CategoryId = 1, Name = "A", Slug = "a", StoreId = null },
                new CategoryModel { CategoryId = 2, Name = "B", Slug = "b", StoreId = null }
            };
            _categoryRepositoryMock.Setup(x => x.ListGlobalAsync()).ReturnsAsync(rows);

            var result = await _sut.ListGlobalAsync();

            Assert.Equal(2, result.Count);
            Assert.All(result, info => Assert.Null(info.StoreId));
            Assert.All(result, info => Assert.True(info.IsGlobal));
        }
    }
}
