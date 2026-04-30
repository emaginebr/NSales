using System.Linq;
using FluentValidation;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Validators
{
    public class ProductTypeFilterInsertInfoValidator : AbstractValidator<ProductTypeFilterInsertInfo>
    {
        private static readonly string[] AllowedDataTypes =
        {
            "text", "integer", "decimal", "boolean", "enum"
        };

        public ProductTypeFilterInsertInfoValidator()
        {
            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Label is required")
                .MaximumLength(120).WithMessage("Label must be at most 120 characters");

            RuleFor(x => x.DataType)
                .NotEmpty().WithMessage("DataType is required")
                .Must(dt => AllowedDataTypes.Contains(dt))
                .WithMessage("DataType must be one of: text, integer, decimal, boolean, enum");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("DisplayOrder must be >= 0");

            When(x => x.DataType == "enum", () =>
            {
                RuleFor(x => x.AllowedValues)
                    .NotNull().WithMessage("AllowedValues is required when DataType is enum")
                    .Must(av => av != null && av.Count >= 1)
                    .WithMessage("AllowedValues must contain at least one item")
                    .Must(av => av == null || av.Distinct().Count() == av.Count)
                    .WithMessage("AllowedValues must be unique")
                    .Must(av => av == null || av.All(v => !string.IsNullOrEmpty(v) && v.Length <= 120))
                    .WithMessage("Each AllowedValue must be non-empty and at most 120 characters");
            });
        }
    }
}
