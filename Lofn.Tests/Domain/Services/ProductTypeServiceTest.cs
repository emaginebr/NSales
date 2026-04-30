using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Lofn.DTO.ProductType;
using Lofn.Infra.Interfaces.Repository;
using Moq;
using Xunit;

namespace Lofn.Tests.Domain.Services
{
    public class ProductTypeServiceTest
    {
        private readonly Mock<IProductTypeRepository<
            ProductTypeModel,
            ProductTypeFilterModel,
            ProductTypeCustomizationGroupModel,
            ProductTypeCustomizationOptionModel>> _repoMock;
        private readonly Mock<IValidator<ProductTypeInsertInfo>> _insertValidatorMock;
        private readonly Mock<IValidator<ProductTypeUpdateInfo>> _updateValidatorMock;
        private readonly Mock<IValidator<ProductTypeFilterInsertInfo>> _filterInsertValidatorMock;
        private readonly Mock<IValidator<ProductTypeFilterUpdateInfo>> _filterUpdateValidatorMock;
        private readonly ProductTypeService _sut;

        public ProductTypeServiceTest()
        {
            _repoMock = new Mock<IProductTypeRepository<
                ProductTypeModel,
                ProductTypeFilterModel,
                ProductTypeCustomizationGroupModel,
                ProductTypeCustomizationOptionModel>>();
            _insertValidatorMock = new Mock<IValidator<ProductTypeInsertInfo>>();
            _updateValidatorMock = new Mock<IValidator<ProductTypeUpdateInfo>>();
            _filterInsertValidatorMock = new Mock<IValidator<ProductTypeFilterInsertInfo>>();
            _filterUpdateValidatorMock = new Mock<IValidator<ProductTypeFilterUpdateInfo>>();

            _insertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<ProductTypeInsertInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _updateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<ProductTypeUpdateInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _filterInsertValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<ProductTypeFilterInsertInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _filterUpdateValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<ProductTypeFilterUpdateInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _sut = new ProductTypeService(
                _repoMock.Object,
                _insertValidatorMock.Object,
                _updateValidatorMock.Object,
                _filterInsertValidatorMock.Object,
                _filterUpdateValidatorMock.Object);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsert_WhenNameIsUnique()
        {
            _repoMock.Setup(r => r.ListAllAsync()).ReturnsAsync(new List<ProductTypeModel>());
            _repoMock.Setup(r => r.InsertAsync(It.IsAny<ProductTypeModel>()))
                .ReturnsAsync((ProductTypeModel m) => { m.ProductTypeId = 10; return m; });

            var dto = new ProductTypeInsertInfo { Name = "Calçado" };
            var result = await _sut.InsertAsync(dto);

            Assert.Equal(10, result.ProductTypeId);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<ProductTypeModel>()), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrow_WhenNameAlreadyExists()
        {
            _repoMock.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<ProductTypeModel> { new ProductTypeModel { ProductTypeId = 5, Name = "Calçado" } });

            var dto = new ProductTypeInsertInfo { Name = "Calçado" };
            await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertAsync(dto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenProductTypeNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProductTypeModel)null);

            var dto = new ProductTypeUpdateInfo { ProductTypeId = 1, Name = "X" };
            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(dto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNameCollidesWithAnotherType()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new ProductTypeModel { ProductTypeId = 1, Name = "Old" });
            _repoMock.Setup(r => r.ListAllAsync())
                .ReturnsAsync(new List<ProductTypeModel>
                {
                    new ProductTypeModel { ProductTypeId = 1, Name = "Old" },
                    new ProductTypeModel { ProductTypeId = 2, Name = "Calçado" }
                });

            var dto = new ProductTypeUpdateInfo { ProductTypeId = 1, Name = "Calçado" };
            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(dto));
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            await _sut.DeleteAsync(7);
            _repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
        }

        [Fact]
        public async Task InsertFilterAsync_ShouldThrow_WhenProductTypeNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProductTypeModel)null);
            var dto = new ProductTypeFilterInsertInfo { Label = "Cor", DataType = "text" };

            await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertFilterAsync(1, dto));
        }

        [Fact]
        public async Task InsertFilterAsync_ShouldThrow_WhenLabelDuplicateForType()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new ProductTypeModel
                {
                    ProductTypeId = 1,
                    Filters = new List<ProductTypeFilterModel>
                    {
                        new ProductTypeFilterModel { FilterId = 5, Label = "Cor" }
                    }
                });
            var dto = new ProductTypeFilterInsertInfo { Label = "Cor", DataType = "text" };

            await Assert.ThrowsAsync<ValidationException>(() => _sut.InsertFilterAsync(1, dto));
        }

        [Fact]
        public async Task InsertFilterAsync_ShouldInsert_WhenLabelUnique()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new ProductTypeModel { ProductTypeId = 1, Filters = new List<ProductTypeFilterModel>() });
            _repoMock.Setup(r => r.InsertFilterAsync(1, It.IsAny<ProductTypeFilterModel>()))
                .ReturnsAsync((long _, ProductTypeFilterModel m) => { m.FilterId = 99; return m; });

            var dto = new ProductTypeFilterInsertInfo { Label = "Cor", DataType = "text" };
            var result = await _sut.InsertFilterAsync(1, dto);

            Assert.Equal(99, result.FilterId);
        }

        [Fact]
        public async Task DeleteFilterAsync_ShouldCallRepository()
        {
            await _sut.DeleteFilterAsync(42);
            _repoMock.Verify(r => r.DeleteFilterAsync(42), Times.Once);
        }
    }
}
