using FluentValidation.TestHelper;
using Lofn.Domain.Validators;
using Lofn.DTO.ProductType;
using Xunit;

namespace Lofn.Tests.Domain.Validators
{
    public class ProductTypeInsertInfoValidatorTests
    {
        private readonly ProductTypeInsertInfoValidator _sut = new ProductTypeInsertInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenNameIsEmpty()
        {
            var result = _sut.TestValidate(new ProductTypeInsertInfo { Name = string.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldHaveError_WhenNameExceedsMaxLength()
        {
            var result = _sut.TestValidate(new ProductTypeInsertInfo { Name = new string('a', 121) });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenNameIsValid()
        {
            var result = _sut.TestValidate(new ProductTypeInsertInfo { Name = "Calçado" });
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
        {
            var result = _sut.TestValidate(new ProductTypeInsertInfo { Name = "X", Description = new string('a', 501) });
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void ShouldNotHaveError_WhenDescriptionIsNull()
        {
            var result = _sut.TestValidate(new ProductTypeInsertInfo { Name = "X", Description = null });
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
