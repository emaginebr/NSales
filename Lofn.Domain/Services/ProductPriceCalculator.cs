using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Models;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Services
{
    public class ProductPriceCalculator
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductPriceCalculator(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<ProductPriceCalculationResult> CalculateAsync(long productId, IList<long> optionIds)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                ThrowValidation("ProductId", "Product not found");

            optionIds ??= new List<long>();
            var basePriceCents = (long)(product.Price * 100);

            AppliedProductTypeResolution resolution = null;
            if (product.CategoryId.HasValue)
                resolution = await _categoryService.GetAppliedProductTypeAsync(product.CategoryId.Value);

            if (resolution?.ProductType == null)
            {
                if (optionIds.Count > 0)
                    ThrowValidation("OptionIds", "Product has no customizations");
                return new ProductPriceCalculationResult
                {
                    ProductId = productId,
                    BasePriceCents = basePriceCents,
                    DeltaTotalCents = 0,
                    TotalCents = basePriceCents
                };
            }

            var groups = resolution.ProductType.CustomizationGroups ?? new List<ProductTypeCustomizationGroupModel>();
            var optionsByGroup = new Dictionary<long, List<ProductTypeCustomizationOptionModel>>();
            var optionLookup = new Dictionary<long, (ProductTypeCustomizationGroupModel Group, ProductTypeCustomizationOptionModel Option)>();

            foreach (var group in groups)
            {
                optionsByGroup[group.GroupId] = new List<ProductTypeCustomizationOptionModel>();
                foreach (var option in group.Options ?? new List<ProductTypeCustomizationOptionModel>())
                {
                    optionLookup[option.OptionId] = (group, option);
                }
            }

            foreach (var optionId in optionIds)
            {
                if (!optionLookup.ContainsKey(optionId))
                    ThrowValidation("OptionIds", $"Option {optionId} does not belong to product type");
                var (g, o) = optionLookup[optionId];
                optionsByGroup[g.GroupId].Add(o);
            }

            // Validate single-select max 1 selection
            foreach (var group in groups)
            {
                if (group.SelectionMode == "single" && optionsByGroup[group.GroupId].Count > 1)
                    ThrowValidation("OptionIds", $"Group '{group.Label}' is single-select; choose at most one option");
            }

            // Validate required groups
            foreach (var group in groups)
            {
                if (group.IsRequired && optionsByGroup[group.GroupId].Count == 0)
                    ThrowValidation("OptionIds", $"Group '{group.Label}' is required");
            }

            var breakdown = new List<PriceBreakdownItem>();
            long deltaTotal = 0;
            foreach (var optionId in optionIds)
            {
                var (g, o) = optionLookup[optionId];
                breakdown.Add(new PriceBreakdownItem
                {
                    OptionId = o.OptionId,
                    GroupLabel = g.Label,
                    OptionLabel = o.Label,
                    PriceDeltaCents = o.PriceDeltaCents
                });
                deltaTotal += o.PriceDeltaCents;
            }

            var total = basePriceCents + deltaTotal;
            if (total < 0)
                ThrowValidation("OptionIds", "Total price cannot be negative");

            return new ProductPriceCalculationResult
            {
                ProductId = productId,
                BasePriceCents = basePriceCents,
                Breakdown = breakdown,
                DeltaTotalCents = deltaTotal,
                TotalCents = total
            };
        }

        private static void ThrowValidation(string property, string message)
        {
            throw new ValidationException(new[] { new ValidationFailure(property, message) });
        }
    }
}
