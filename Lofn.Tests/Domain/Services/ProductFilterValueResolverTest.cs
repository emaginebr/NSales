using System.Collections.Generic;
using System.Threading.Tasks;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Moq;
using Xunit;

namespace Lofn.Tests.Domain.Services
{
    public class ProductFilterValueResolverTest
    {
        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly ProductFilterValueResolver _sut;

        public ProductFilterValueResolverTest()
        {
            _sut = new ProductFilterValueResolver(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task Resolve_WhenNoAppliedType_ShouldIgnoreAllInputs()
        {
            _categoryServiceMock.Setup(s => s.GetAppliedProductTypeAsync(1))
                .ReturnsAsync((AppliedProductTypeResolution)null);

            var result = await _sut.ResolveAsync(1, new List<(long, string)> { (10, "x") });

            Assert.Empty(result.Resolved);
            Assert.Single(result.IgnoredFilterIds);
            Assert.Equal(10, result.IgnoredFilterIds[0]);
            Assert.Empty(result.MissingRequiredLabels);
        }

        [Fact]
        public async Task Resolve_WhenRequiredFilterMissing_ShouldReportInMissingRequired()
        {
            _categoryServiceMock.Setup(s => s.GetAppliedProductTypeAsync(1))
                .ReturnsAsync(new AppliedProductTypeResolution
                {
                    OriginCategoryId = 1,
                    ProductType = new ProductTypeModel
                    {
                        ProductTypeId = 5,
                        Filters = new List<ProductTypeFilterModel>
                        {
                            new ProductTypeFilterModel { FilterId = 10, Label = "Cor", DataType = "enum", IsRequired = true, AllowedValues = new List<string>{"Red"} }
                        }
                    }
                });

            var result = await _sut.ResolveAsync(1, new List<(long, string)>());

            Assert.Contains("Cor", result.MissingRequiredLabels);
        }

        [Fact]
        public async Task Resolve_WhenIntegerValueNotParseable_ShouldReportInvalidError()
        {
            _categoryServiceMock.Setup(s => s.GetAppliedProductTypeAsync(1))
                .ReturnsAsync(new AppliedProductTypeResolution
                {
                    OriginCategoryId = 1,
                    ProductType = new ProductTypeModel
                    {
                        ProductTypeId = 5,
                        Filters = new List<ProductTypeFilterModel>
                        {
                            new ProductTypeFilterModel { FilterId = 10, Label = "Tamanho", DataType = "integer" }
                        }
                    }
                });

            var result = await _sut.ResolveAsync(1, new List<(long, string)> { (10, "abc") });

            Assert.NotEmpty(result.InvalidValueErrors);
        }

        [Fact]
        public async Task Resolve_WhenEnumValueNotInAllowedList_ShouldReportInvalid()
        {
            _categoryServiceMock.Setup(s => s.GetAppliedProductTypeAsync(1))
                .ReturnsAsync(new AppliedProductTypeResolution
                {
                    OriginCategoryId = 1,
                    ProductType = new ProductTypeModel
                    {
                        ProductTypeId = 5,
                        Filters = new List<ProductTypeFilterModel>
                        {
                            new ProductTypeFilterModel { FilterId = 10, Label = "Cor", DataType = "enum", AllowedValues = new List<string>{"A","B"} }
                        }
                    }
                });

            var result = await _sut.ResolveAsync(1, new List<(long, string)> { (10, "C") });

            Assert.NotEmpty(result.InvalidValueErrors);
        }

        [Fact]
        public async Task Resolve_WhenFilterIdUnknown_ShouldGoToIgnoredList()
        {
            _categoryServiceMock.Setup(s => s.GetAppliedProductTypeAsync(1))
                .ReturnsAsync(new AppliedProductTypeResolution
                {
                    OriginCategoryId = 1,
                    ProductType = new ProductTypeModel
                    {
                        ProductTypeId = 5,
                        Filters = new List<ProductTypeFilterModel>
                        {
                            new ProductTypeFilterModel { FilterId = 10, Label = "Cor", DataType = "text" }
                        }
                    }
                });

            var result = await _sut.ResolveAsync(1, new List<(long, string)> { (10, "Red"), (99, "Foo") });

            Assert.Single(result.Resolved);
            Assert.Equal(10, result.Resolved[0].FilterId);
            Assert.Single(result.IgnoredFilterIds);
            Assert.Equal(99, result.IgnoredFilterIds[0]);
        }
    }
}
