using FluentValidation;
using Lofn.DTO.Category;

namespace Lofn.Domain.Validators
{
    public class CategoryGlobalInsertInfoValidator : AbstractValidator<CategoryGlobalInsertInfo>
    {
        public CategoryGlobalInsertInfoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(120).WithMessage("Name must be at most 120 characters");
        }
    }
}
