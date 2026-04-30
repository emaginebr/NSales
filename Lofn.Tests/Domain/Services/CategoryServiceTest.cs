using Xunit;
using Moq;
using Lofn.Domain.Core;
using Lofn.Domain.Services;
using Lofn.Domain.Models;
using Lofn.DTO.Category;
using Lofn.Infra.Interfaces.Repository;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Tests.Domain.Services
{
    public class CategoryServiceTest
    {
        private readonly Mock<ISlugGenerator> _slugGeneratorMock;
        private readonly Mock<ICategoryRepository<CategoryModel>> _categoryRepositoryMock;
        private readonly Mock<IStoreRepository<StoreModel>> _storeRepositoryMock;
        private readonly Mock<IProductTypeRepository<ProductTypeModel, ProductTypeFilterModel, ProductTypeCustomizationGroupModel, ProductTypeCustomizationOptionModel>> _productTypeRepositoryMock;
        private readonly Mock<IValidator<CategoryInsertInfo>> _insertValidatorMock;
        private readonly Mock<IValidator<CategoryUpdateInfo>> _updateValidatorMock;
        private readonly Mock<IValidator<CategoryGlobalInsertInfo>> _globalInsertValidatorMock;
        private readonly Mock<IValidator<CategoryGlobalUpdateInfo>> _globalUpdateValidatorMock;
        private readonly CategoryService _sut;

        private readonly StoreModel _validStore = new StoreModel { StoreId = 1, OwnerId = 1, Name = "Loja", Slug = "loja" };

        public CategoryServiceTest()
        {
            _slugGeneratorMock = new Mock<ISlugGenerator>();
            _categoryRepositoryMock = new Mock<ICategoryRepository<CategoryModel>>();
            _storeRepositoryMock = new Mock<IStoreRepository<StoreModel>>();
            _productTypeRepositoryMock = new Mock<IProductTypeRepository<ProductTypeModel, ProductTypeFilterModel, ProductTypeCustomizationGroupModel, ProductTypeCustomizationOptionModel>>();
            _insertValidatorMock = new Mock<IValidator<CategoryInsertInfo>>();
            _updateValidatorMock = new Mock<IValidator<CategoryUpdateInfo>>();
            _globalInsertValidatorMock = new Mock<IValidator<CategoryGlobalInsertInfo>>();
            _globalUpdateValidatorMock = new Mock<IValidator<CategoryGlobalUpdateInfo>>();
            _insertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryInsertInfo>>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _updateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryUpdateInfo>>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _globalInsertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalInsertInfo>>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _globalUpdateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalUpdateInfo>>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _sut = new CategoryService(
                _slugGeneratorMock.Object,
                _categoryRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _productTypeRepositoryMock.Object,
                _insertValidatorMock.Object,
                _updateValidatorMock.Object,
                _globalInsertValidatorMock.Object,
                _globalUpdateValidatorMock.Object
            );
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenStoreIdIsInvalid()
        {
            var category = new CategoryInsertInfo { Name = "Cat" };

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.InsertAsync(category, 0, 1));
            Assert.Equal("StoreId is required", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowUnauthorized_WhenUserIsNotOwner()
        {
            var category = new CategoryInsertInfo { Name = "Cat" };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_validStore);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.InsertAsync(category, 1, 999));
        }

        [Fact]
        public async Task InsertAsync_ShouldCreateCategory_WhenValid()
        {
            var category = new CategoryInsertInfo { Name = "Eletrônicos" };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_validStore);
            _slugGeneratorMock.Setup(x => x.Generate("Eletrônicos")).Returns("eletronicos");
            _categoryRepositoryMock.Setup(x => x.ExistSlugInTenantAsync(null, "eletronicos")).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<CategoryModel>()))
                .ReturnsAsync(new CategoryModel { CategoryId = 1, Name = "Eletrônicos", Slug = "eletronicos", StoreId = 1 });

            var result = await _sut.InsertAsync(category, 1, 1);

            Assert.Equal("Eletrônicos", result.Name);
            Assert.Equal((long?)1, result.StoreId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCategoryNotFound()
        {
            var category = new CategoryUpdateInfo { CategoryId = 99, Name = "Nova" };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_validStore);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((CategoryModel)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(category, 1, 1));
            Assert.Equal("Category not found", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorized_WhenCategoryBelongsToDifferentStore()
        {
            var category = new CategoryUpdateInfo { CategoryId = 1, Name = "Nova" };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_validStore);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new CategoryModel { CategoryId = 1, StoreId = 999, Name = "Outra" });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(category, 1, 1));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenCategoryNotFound()
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_validStore);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((CategoryModel)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(99, 1, 1));
            Assert.Equal("Category not found", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDelete_WhenValid()
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_validStore);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new CategoryModel { CategoryId = 1, StoreId = 1, Name = "Cat" });

            await _sut.DeleteAsync(1, 1, 1);

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryNotFound()
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(_validStore);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((CategoryModel)null);

            var result = await _sut.GetByIdAsync(99, 1, 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task ListByStoreAsync_ShouldReturnCategories()
        {
            var models = new List<CategoryModel>
            {
                new CategoryModel { CategoryId = 1, Name = "A", Slug = "a", StoreId = 1 },
                new CategoryModel { CategoryId = 2, Name = "B", Slug = "b", StoreId = 1 }
            };
            _categoryRepositoryMock.Setup(x => x.ListByStoreAsync(1)).ReturnsAsync(models);

            var result = await _sut.ListByStoreAsync(1);

            Assert.Equal(2, result.Count);
        }
    }
}
