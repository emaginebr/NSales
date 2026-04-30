using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lofn.Domain.Core;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Lofn.DTO.Category;
using Lofn.Infra.Interfaces.Repository;
using Moq;
using Xunit;

namespace Lofn.Tests.Domain.Services
{
    public class CategoryServiceProductTypeTest
    {
        private readonly Mock<ISlugGenerator> _slugGeneratorMock = new();
        private readonly Mock<ICategoryRepository<CategoryModel>> _categoryRepositoryMock = new();
        private readonly Mock<IStoreRepository<StoreModel>> _storeRepositoryMock = new();
        private readonly Mock<IProductTypeRepository<ProductTypeModel, ProductTypeFilterModel, ProductTypeCustomizationGroupModel, ProductTypeCustomizationOptionModel>> _productTypeRepositoryMock = new();
        private readonly Mock<IValidator<CategoryInsertInfo>> _insertValidatorMock = new();
        private readonly Mock<IValidator<CategoryUpdateInfo>> _updateValidatorMock = new();
        private readonly Mock<IValidator<CategoryGlobalInsertInfo>> _globalInsertValidatorMock = new();
        private readonly Mock<IValidator<CategoryGlobalUpdateInfo>> _globalUpdateValidatorMock = new();
        private readonly CategoryService _sut;

        public CategoryServiceProductTypeTest()
        {
            _insertValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryInsertInfo>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _updateValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryUpdateInfo>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _globalInsertValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalInsertInfo>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _globalUpdateValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<CategoryGlobalUpdateInfo>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            _sut = new CategoryService(
                _slugGeneratorMock.Object,
                _categoryRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _productTypeRepositoryMock.Object,
                _insertValidatorMock.Object,
                _updateValidatorMock.Object,
                _globalInsertValidatorMock.Object,
                _globalUpdateValidatorMock.Object);
        }

        [Fact]
        public async Task LinkProductTypeAsync_ShouldThrow_WhenCategoryNotFound()
        {
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((CategoryModel)null);

            await Assert.ThrowsAsync<ValidationException>(() => _sut.LinkProductTypeAsync(1, 7));
        }

        [Fact]
        public async Task LinkProductTypeAsync_ShouldThrow_WhenProductTypeNotFound()
        {
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new CategoryModel { CategoryId = 1 });
            _productTypeRepositoryMock.Setup(r => r.ExistsAsync(7)).ReturnsAsync(false);

            await Assert.ThrowsAsync<ValidationException>(() => _sut.LinkProductTypeAsync(1, 7));
        }

        [Fact]
        public async Task LinkProductTypeAsync_ShouldCallUpdate_WhenInputsValid()
        {
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new CategoryModel { CategoryId = 1 });
            _productTypeRepositoryMock.Setup(r => r.ExistsAsync(7)).ReturnsAsync(true);

            await _sut.LinkProductTypeAsync(1, 7);

            _categoryRepositoryMock.Verify(r => r.UpdateProductTypeIdAsync(1, 7), Times.Once);
        }

        [Fact]
        public async Task UnlinkProductTypeAsync_ShouldCallUpdateWithNull()
        {
            _categoryRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new CategoryModel { CategoryId = 1 });

            await _sut.UnlinkProductTypeAsync(1);

            _categoryRepositoryMock.Verify(r => r.UpdateProductTypeIdAsync(1, null), Times.Once);
        }

        [Fact]
        public async Task GetAppliedProductTypeAsync_ShouldReturnNull_WhenNoAncestorIsTyped()
        {
            _categoryRepositoryMock.Setup(r => r.GetAppliedProductTypeAsync(5))
                .ReturnsAsync(((long?)null, (long?)null));

            var result = await _sut.GetAppliedProductTypeAsync(5);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAppliedProductTypeAsync_ShouldReturnTypeAndOrigin_WhenAncestorIsTyped()
        {
            _categoryRepositoryMock.Setup(r => r.GetAppliedProductTypeAsync(5))
                .ReturnsAsync(((long?)10, (long?)2));
            _productTypeRepositoryMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new ProductTypeModel { ProductTypeId = 10, Name = "Calçado" });

            var result = await _sut.GetAppliedProductTypeAsync(5);

            Assert.NotNull(result);
            Assert.Equal(10, result.ProductType.ProductTypeId);
            Assert.Equal(2, result.OriginCategoryId);
        }
    }
}
