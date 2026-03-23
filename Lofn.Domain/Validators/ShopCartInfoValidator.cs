using FluentValidation;
using Lofn.DTO.ShopCart;

namespace Lofn.Domain.Validators
{
    public class ShopCartInfoValidator : AbstractValidator<ShopCartInfo>
    {
        public ShopCartInfoValidator()
        {
            RuleFor(x => x.User)
                .NotNull().WithMessage("User is required");

            RuleFor(x => x.User.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than 0")
                .When(x => x.User != null);

            RuleFor(x => x.Address)
                .NotNull().WithMessage("Address is required");

            RuleFor(x => x.Address)
                .SetValidator(new ShopCartAddressInfoValidator())
                .When(x => x.Address != null);

            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items is required")
                .Must(items => items != null && items.Count > 0).WithMessage("Items must have at least 1 item");

            RuleForEach(x => x.Items)
                .SetValidator(new ShopCartItemInfoValidator())
                .When(x => x.Items != null && x.Items.Count > 0);
        }
    }
}
