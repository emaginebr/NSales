using System.Linq;
using FluentValidation;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Validators
{
    public class ProductTypeFilterUpdateInfoValidator : AbstractValidator<ProductTypeFilterUpdateInfo>
    {
        public ProductTypeFilterUpdateInfoValidator()
        {
            RuleFor(x => x.FilterId)
                .GreaterThan(0).WithMessage("FilterId must be greater than 0");

            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Label is required")
                .MaximumLength(120).WithMessage("Label must be at most 120 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be >= 0");

            When(x => x.AllowedValues != null, () =>
            {
                RuleFor(x => x.AllowedValues)
                    .Must(av => av.Distinct().Count() == av.Count)
                    .WithMessage("AllowedValues must be unique")
                    .Must(av => av.All(v => !string.IsNullOrEmpty(v) && v.Length <= 120))
                    .WithMessage("Each AllowedValue must be non-empty and at most 120 characters");
            });
        }
    }
}
