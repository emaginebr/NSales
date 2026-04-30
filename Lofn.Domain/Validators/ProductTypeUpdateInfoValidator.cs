using FluentValidation;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Validators
{
    public class ProductTypeUpdateInfoValidator : AbstractValidator<ProductTypeUpdateInfo>
    {
        public ProductTypeUpdateInfoValidator()
        {
            RuleFor(x => x.ProductTypeId)
                .GreaterThan(0).WithMessage("ProductTypeId must be greater than 0");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(120).WithMessage("Name must be at most 120 characters");

            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(500).WithMessage("Description must be at most 500 characters");
            });
        }
    }
}
