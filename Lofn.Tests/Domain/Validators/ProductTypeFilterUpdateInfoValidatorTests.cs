using FluentValidation.TestHelper;
using Lofn.Domain.Validators;
using Lofn.DTO.ProductType;
using Xunit;

namespace Lofn.Tests.Domain.Validators
{
    public class ProductTypeFilterUpdateInfoValidatorTests
    {
        private readonly ProductTypeFilterUpdateInfoValidator _sut = new ProductTypeFilterUpdateInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenFilterIdIsZero()
        {
            var result = _sut.TestValidate(new ProductTypeFilterUpdateInfo { FilterId = 0, Label = "X" });
            result.ShouldHaveValidationErrorFor(x => x.FilterId);
        }

        [Fact]
        public void ShouldHaveError_WhenLabelIsEmpty()
        {
            var result = _sut.TestValidate(new ProductTypeFilterUpdateInfo { FilterId = 1, Label = string.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Label);
        }

        [Fact]
        public void ShouldHaveError_WhenAllowedValuesHaveDuplicates()
        {
            var result = _sut.TestValidate(new ProductTypeFilterUpdateInfo
            {
                FilterId = 1,
                Label = "Cor",
                AllowedValues = new[] { "A", "A" }
            });
            result.ShouldHaveValidationErrorFor(x => x.AllowedValues);
        }

        [Fact]
        public void ShouldNotHaveError_WhenAllowedValuesIsNull()
        {
            var result = _sut.TestValidate(new ProductTypeFilterUpdateInfo
            {
                FilterId = 1,
                Label = "Cor",
                AllowedValues = null
            });
            result.ShouldNotHaveValidationErrorFor(x => x.AllowedValues);
        }

        [Fact]
        public void ShouldHaveError_WhenDisplayOrderIsNegative()
        {
            var result = _sut.TestValidate(new ProductTypeFilterUpdateInfo
            {
                FilterId = 1,
                Label = "Cor",
                DisplayOrder = -1
            });
            result.ShouldHaveValidationErrorFor(x => x.DisplayOrder);
        }
    }
}
