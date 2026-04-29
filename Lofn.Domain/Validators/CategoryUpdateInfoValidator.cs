using FluentValidation;
using Lofn.DTO.Category;

namespace Lofn.Domain.Validators
{
    public class CategoryUpdateInfoValidator : AbstractValidator<CategoryUpdateInfo>
    {
        public CategoryUpdateInfoValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("CategoryId must be greater than 0");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(120).WithMessage("Name must be at most 120 characters");

            When(x => x.ParentCategoryId.HasValue, () =>
            {
                RuleFor(x => x.ParentCategoryId.Value)
                    .GreaterThan(0).WithMessage("ParentCategoryId must be greater than 0 when provided");
            });
        }
    }
}
