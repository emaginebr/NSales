using FluentValidation;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Validators
{
    public class CustomizationOptionInsertInfoValidator : AbstractValidator<CustomizationOptionInsertInfo>
    {
        public CustomizationOptionInsertInfoValidator()
        {
            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Label is required")
                .MaximumLength(120).WithMessage("Label must be at most 120 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be >= 0");
        }
    }

    public class CustomizationOptionUpdateInfoValidator : AbstractValidator<CustomizationOptionUpdateInfo>
    {
        public CustomizationOptionUpdateInfoValidator()
        {
            RuleFor(x => x.OptionId).GreaterThan(0).WithMessage("OptionId must be greater than 0");
            RuleFor(x => x.Label).NotEmpty().MaximumLength(120);
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        }
    }
}
