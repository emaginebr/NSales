using FluentValidation;
using Lofn.DTO.ShopCart;

namespace Lofn.Domain.Validators
{
    public class ShopCartAddressInfoValidator : AbstractValidator<ShopCartAddressInfo>
    {
        public ShopCartAddressInfoValidator()
        {
            RuleFor(x => x.ZipCode)
                .NotEmpty().WithMessage("ZipCode is required");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required");

            RuleFor(x => x.Neighborhood)
                .NotEmpty().WithMessage("Neighborhood is required");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required");

            RuleFor(x => x.State)
                .NotEmpty().WithMessage("State is required");
        }
    }
}
