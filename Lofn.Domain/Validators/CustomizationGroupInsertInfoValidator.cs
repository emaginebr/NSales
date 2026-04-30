using System.Linq;
using FluentValidation;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Validators
{
    public class CustomizationGroupInsertInfoValidator : AbstractValidator<CustomizationGroupInsertInfo>
    {
        private static readonly string[] AllowedModes = { "single", "multi" };

        public CustomizationGroupInsertInfoValidator()
        {
            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Label is required")
                .MaximumLength(120).WithMessage("Label must be at most 120 characters");

            RuleFor(x => x.SelectionMode)
                .Must(m => AllowedModes.Contains(m))
                .WithMessage("SelectionMode must be 'single' or 'multi'");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be >= 0");
        }
    }

    public class CustomizationGroupUpdateInfoValidator : AbstractValidator<CustomizationGroupUpdateInfo>
    {
        private static readonly string[] AllowedModes = { "single", "multi" };

        public CustomizationGroupUpdateInfoValidator()
        {
            RuleFor(x => x.GroupId).GreaterThan(0).WithMessage("GroupId must be greater than 0");
            RuleFor(x => x.Label).NotEmpty().MaximumLength(120);
            RuleFor(x => x.SelectionMode)
                .Must(m => AllowedModes.Contains(m))
                .WithMessage("SelectionMode must be 'single' or 'multi'");
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        }
    }
}
