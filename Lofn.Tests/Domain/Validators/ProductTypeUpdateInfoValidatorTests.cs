using FluentValidation.TestHelper;
using Lofn.Domain.Validators;
using Lofn.DTO.ProductType;
using Xunit;

namespace Lofn.Tests.Domain.Validators
{
    public class ProductTypeUpdateInfoValidatorTests
    {
        private readonly ProductTypeUpdateInfoValidator _sut = new ProductTypeUpdateInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenProductTypeIdIsZero()
        {
            var result = _sut.TestValidate(new ProductTypeUpdateInfo { ProductTypeId = 0, Name = "X" });
            result.ShouldHaveValidationErrorFor(x => x.ProductTypeId);
        }

        [Fact]
        public void ShouldHaveError_WhenNameIsEmpty()
        {
            var result = _sut.TestValidate(new ProductTypeUpdateInfo { ProductTypeId = 1, Name = string.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenAllFieldsAreValid()
        {
            var result = _sut.TestValidate(new ProductTypeUpdateInfo { ProductTypeId = 1, Name = "Calçado", Description = "OK" });
            result.ShouldNotHaveValidationErrorFor(x => x.ProductTypeId);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
        {
            var result = _sut.TestValidate(new ProductTypeUpdateInfo
            {
                ProductTypeId = 1,
                Name = "X",
                Description = new string('a', 501)
            });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}
