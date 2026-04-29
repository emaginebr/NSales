using FluentValidation.TestHelper;
using Lofn.Domain.Validators;
using Lofn.DTO.Category;
using Xunit;

namespace Lofn.Tests.Domain.Validators
{
    public class CategoryUpdateInfoValidatorTest
    {
        private readonly CategoryUpdateInfoValidator _sut = new CategoryUpdateInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenCategoryIdIsZero()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 0, Name = "X" });
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void ShouldHaveError_WhenCategoryIdIsNegative()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = -1, Name = "X" });
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void ShouldHaveError_WhenNameIsEmpty()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 1, Name = string.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldHaveError_WhenNameExceedsMaxLength()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 1, Name = new string('a', 121) });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenAllValid()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 7, Name = "Roupas" });
            result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsNull()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = null });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsPositive()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = 9 });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsZero()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = 0 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsNegative()
        {
            var result = _sut.TestValidate(new CategoryUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = -2 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }
    }
}
