using FluentValidation.TestHelper;
using Lofn.Domain.Validators;
using Lofn.DTO.Category;
using Xunit;

namespace Lofn.Tests.Domain.Validators
{
    public class CategoryGlobalInsertInfoValidatorTest
    {
        private readonly CategoryGlobalInsertInfoValidator _sut = new CategoryGlobalInsertInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenNameIsEmpty()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = string.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldHaveError_WhenNameIsNull()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = null });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldHaveError_WhenNameExceedsMaxLength()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = new string('a', 121) });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenNameIsValid()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = "Eletrônicos" });
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenNameIsExactly120Chars()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = new string('a', 120) });
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsNull()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = "X", ParentCategoryId = null });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsPositive()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = "X", ParentCategoryId = 7 });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsZero()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = "X", ParentCategoryId = 0 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsNegative()
        {
            var result = _sut.TestValidate(new CategoryGlobalInsertInfo { Name = "X", ParentCategoryId = -3 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }
    }

    public class CategoryGlobalUpdateInfoValidatorTest
    {
        private readonly CategoryGlobalUpdateInfoValidator _sut = new CategoryGlobalUpdateInfoValidator();

        [Fact]
        public void ShouldHaveError_WhenCategoryIdIsZero()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 0, Name = "X" });
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void ShouldHaveError_WhenCategoryIdIsNegative()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = -1, Name = "X" });
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void ShouldHaveError_WhenNameIsEmpty()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 1, Name = string.Empty });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldHaveError_WhenNameExceedsMaxLength()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 1, Name = new string('a', 121) });
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenAllValid()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 42, Name = "Periféricos" });
            result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsNull()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = null });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldNotHaveError_WhenParentCategoryIdIsPositive()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = 9 });
            result.ShouldNotHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsZero()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = 0 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }

        [Fact]
        public void ShouldHaveError_WhenParentCategoryIdIsNegative()
        {
            var result = _sut.TestValidate(new CategoryGlobalUpdateInfo { CategoryId = 1, Name = "X", ParentCategoryId = -1 });
            result.ShouldHaveValidationErrorFor("ParentCategoryId.Value");
        }
    }
}
