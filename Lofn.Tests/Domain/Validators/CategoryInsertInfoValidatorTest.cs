using FluentValidation.TestHelper;
using Lofn.Domain.Validators;
using Lofn.DTO.Category;
using Xunit;

namespace Lofn.Tests.Domain.Validators
{
    public class CategoryInsertInfoValidatorTest
    {
        private readonly CategoryInsertInfoValidator _sut = new CategoryInsertInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenNameIsEmpty()
        {
            var result = _sut.TestValidate(new CategoryInsertInfo { Name = string.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldHaveError_WhenNameExceedsMaxLength()
        {
            var result = _sut.TestValidate(new CategoryInsertInfo { Name = new string('a', 121) });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenNameIsValid()
        {
            var result = _sut.TestValidate(new CategoryInsertInfo { Name = "Roupas" });
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsNull()
        {
            var result = _sut.TestValidate(new CategoryInsertInfo { Name = "X", ParentCategoryId = null });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsPositive()
        {
            var result = _sut.TestValidate(new CategoryInsertInfo { Name = "X", ParentCategoryId = 5 });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsZero()
        {
            var result = _sut.TestValidate(new CategoryInsertInfo { Name = "X", ParentCategoryId = 0 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsNegative()
        {
            var result = _sut.TestValidate(new CategoryInsertInfo { Name = "X", ParentCategoryId = -2 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }
    }
}
