using FluentValidation.TestHelper;
using Lofn.Domain.Validators;
using Lofn.DTO.ProductType;
using Xunit;

namespace Lofn.Tests.Domain.Validators
{
    public class ProductTypeFilterInsertInfoValidatorTests
    {
        private readonly ProductTypeFilterInsertInfoValidator _sut = new ProductTypeFilterInsertInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenLabelIsEmpty()
        {
            var result = _sut.TestValidate(new ProductTypeFilterInsertInfo { Label = string.Empty, DataType = "text" });
            result.ShouldHaveValidationErrorFor(x => x.Label);
        }

        [Fact]
        public void ShouldHaveError_WhenDataTypeIsInvalid()
        {
            var result = _sut.TestValidate(new ProductTypeFilterInsertInfo { Label = "Cor", DataType = "invalid" });
            result.ShouldHaveValidationErrorFor(x => x.DataType);
        }

        [Theory]
        [InlineData("text")]
        [InlineData("integer")]
        [InlineData("decimal")]
        [InlineData("boolean")]
        public void ShouldNotHaveError_WhenDataTypeIsAcceptedAndNotEnum(string dataType)
        {
            var result = _sut.TestValidate(new ProductTypeFilterInsertInfo { Label = "Cor", DataType = dataType });
            result.ShouldNotHaveValidationErrorFor(x => x.DataType);
        }

        [Fact]
        public void ShouldHaveError_WhenEnumWithoutAllowedValues()
        {
            var result = _sut.TestValidate(new ProductTypeFilterInsertInfo
            {
                Label = "Cor",
                DataType = "enum",
                AllowedValues = null
            });
            result.ShouldHaveValidationErrorFor(x => x.AllowedValues);
        }

        [Fact]
        public void ShouldHaveError_WhenEnumWithDuplicateAllowedValues()
        {
            var result = _sut.TestValidate(new ProductTypeFilterInsertInfo
            {
                Label = "Cor",
                DataType = "enum",
                AllowedValues = new[] { "Vermelho", "Vermelho" }
            });
            result.ShouldHaveValidationErrorFor(x => x.AllowedValues);
        }

        [Fact]
        public void ShouldNotHaveError_WhenEnumWithUniqueAllowedValues()
        {
            var result = _sut.TestValidate(new ProductTypeFilterInsertInfo
            {
                Label = "Cor",
                DataType = "enum",
                AllowedValues = new[] { "Vermelho", "Azul" }
            });
            result.ShouldNotHaveValidationErrorFor(x => x.AllowedValues);
        }

        [Fact]
        public void ShouldHaveError_WhenDisplayOrderIsNegative()
        {
            var result = _sut.TestValidate(new ProductTypeFilterInsertInfo
            {
                Label = "Cor",
                DataType = "text",
                DisplayOrder = -1
            });
            result.ShouldHaveValidationErrorFor(x => x.DisplayOrder);
        }
    }
}
