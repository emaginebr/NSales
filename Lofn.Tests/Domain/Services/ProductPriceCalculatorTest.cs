using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Moq;
using Xunit;

namespace Lofn.Tests.Domain.Services
{
    public class ProductPriceCalculatorTest
    {
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly ProductPriceCalculator _sut;

        public ProductPriceCalculatorTest()
        {
            _sut = new ProductPriceCalculator(_productServiceMock.Object, _categoryServiceMock.Object);
        }

        private void GivenProduct(double price, long? categoryId)
        {
            _productServiceMock.Setup(p => p.GetByIdAsync(1))
                .ReturnsAsync(new ProductModel { ProductId = 1, Price = price, CategoryId = categoryId });
        }

        private void GivenAppliedType(ProductTypeCustomizationGroupModel group)
        {
            _categoryServiceMock.Setup(c => c.GetAppliedProductTypeAsync(99))
                .ReturnsAsync(new AppliedProductTypeResolution
                {
                    OriginCategoryId = 99,
                    ProductType = new ProductTypeModel
                    {
                        ProductTypeId = 5,
                        CustomizationGroups = new List<ProductTypeCustomizationGroupModel> { group }
                    }
                });
        }

        [Fact]
        public async Task Calculate_WithValidOption_AddsDeltaToBasePrice()
        {
            GivenProduct(300, 99);
            GivenAppliedType(new ProductTypeCustomizationGroupModel
            {
                GroupId = 1,
                Label = "Processor",
                SelectionMode = "single",
                Options = new List<ProductTypeCustomizationOptionModel>
                {
                    new ProductTypeCustomizationOptionModel { OptionId = 10, Label = "i7", PriceDeltaCents = 90000, GroupId = 1 }
                }
            });

            var result = await _sut.CalculateAsync(1, new List<long> { 10 });
            Assert.Equal(30000, result.BasePriceCents);
            Assert.Equal(90000, result.DeltaTotalCents);
            Assert.Equal(120000, result.TotalCents);
        }

        [Fact]
        public async Task Calculate_WithTwoOptionsInSingleGroup_Throws()
        {
            GivenProduct(300, 99);
            GivenAppliedType(new ProductTypeCustomizationGroupModel
            {
                GroupId = 1,
                Label = "Processor",
                SelectionMode = "single",
                Options = new List<ProductTypeCustomizationOptionModel>
                {
                    new ProductTypeCustomizationOptionModel { OptionId = 10, GroupId = 1 },
                    new ProductTypeCustomizationOptionModel { OptionId = 11, GroupId = 1 }
                }
            });

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CalculateAsync(1, new List<long> { 10, 11 }));
        }

        [Fact]
        public async Task Calculate_WithMissingRequiredGroup_Throws()
        {
            GivenProduct(300, 99);
            GivenAppliedType(new ProductTypeCustomizationGroupModel
            {
                GroupId = 1,
                Label = "Processor",
                SelectionMode = "single",
                IsRequired = true,
                Options = new List<ProductTypeCustomizationOptionModel>
                {
                    new ProductTypeCustomizationOptionModel { OptionId = 10, GroupId = 1 }
                }
            });

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CalculateAsync(1, new List<long>()));
        }

        [Fact]
        public async Task Calculate_NoAppliedType_ReturnsBasePriceWhenNoOptions()
        {
            GivenProduct(300, 99);
            _categoryServiceMock.Setup(c => c.GetAppliedProductTypeAsync(99))
                .ReturnsAsync((AppliedProductTypeResolution)null);

            var result = await _sut.CalculateAsync(1, new List<long>());
            Assert.Equal(30000, result.TotalCents);
        }

        [Fact]
        public async Task Calculate_NoAppliedType_WithOptions_Throws()
        {
            GivenProduct(300, 99);
            _categoryServiceMock.Setup(c => c.GetAppliedProductTypeAsync(99))
                .ReturnsAsync((AppliedProductTypeResolution)null);

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CalculateAsync(1, new List<long> { 10 }));
        }

        [Fact]
        public async Task Calculate_OptionNotInType_Throws()
        {
            GivenProduct(300, 99);
            GivenAppliedType(new ProductTypeCustomizationGroupModel
            {
                GroupId = 1,
                Label = "Processor",
                SelectionMode = "single",
                Options = new List<ProductTypeCustomizationOptionModel>
                {
                    new ProductTypeCustomizationOptionModel { OptionId = 10, GroupId = 1 }
                }
            });

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CalculateAsync(1, new List<long> { 999 }));
        }

        [Fact]
        public async Task Calculate_NegativeTotal_Throws()
        {
            GivenProduct(1, 99);
            GivenAppliedType(new ProductTypeCustomizationGroupModel
            {
                GroupId = 1,
                Label = "Discount",
                SelectionMode = "single",
                Options = new List<ProductTypeCustomizationOptionModel>
                {
                    new ProductTypeCustomizationOptionModel { OptionId = 10, GroupId = 1, PriceDeltaCents = -1000 }
                }
            });

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CalculateAsync(1, new List<long> { 10 }));
        }
    }
}
