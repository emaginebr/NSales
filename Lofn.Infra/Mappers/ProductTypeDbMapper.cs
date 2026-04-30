using System.Linq;
using Lofn.Domain.Models;
using Lofn.Infra.Context;

namespace Lofn.Infra.Mappers
{
    /// <summary>
    /// Maps the ProductType aggregate (type + filters + allowed_values + customization
    /// groups + options) between the EF entity tree and the domain model tree.
    /// </summary>
    public static class ProductTypeDbMapper
    {
        public static ProductTypeModel ToModel(ProductType row)
        {
            var model = new ProductTypeModel
            {
                ProductTypeId = row.ProductTypeId,
                Name = row.Name,
                Description = row.Description,
                CreatedAt = row.CreatedAt,
                UpdatedAt = row.UpdatedAt,
                Filters = row.Filters
                    .OrderBy(f => f.DisplayOrder).ThenBy(f => f.FilterId)
                    .Select(ToFilterModel)
                    .ToList(),
                CustomizationGroups = row.CustomizationGroups
                    .OrderBy(g => g.DisplayOrder).ThenBy(g => g.GroupId)
                    .Select(ToGroupModel)
                    .ToList()
            };
            return model;
        }

        public static ProductTypeFilterModel ToFilterModel(ProductTypeFilter row)
        {
            return new ProductTypeFilterModel
            {
                FilterId = row.FilterId,
                ProductTypeId = row.ProductTypeId,
                Label = row.Label,
                DataType = row.DataType,
                IsRequired = row.IsRequired,
                DisplayOrder = row.DisplayOrder,
                AllowedValues = row.AllowedValues
                    .OrderBy(v => v.DisplayOrder).ThenBy(v => v.AllowedValueId)
                    .Select(v => v.Value)
                    .ToList()
            };
        }

        public static ProductTypeCustomizationGroupModel ToGroupModel(ProductTypeCustomizationGroup row)
        {
            return new ProductTypeCustomizationGroupModel
            {
                GroupId = row.GroupId,
                ProductTypeId = row.ProductTypeId,
                Label = row.Label,
                SelectionMode = row.SelectionMode,
                IsRequired = row.IsRequired,
                DisplayOrder = row.DisplayOrder,
                Options = row.Options
                    .OrderBy(o => o.DisplayOrder).ThenBy(o => o.OptionId)
                    .Select(ToOptionModel)
                    .ToList()
            };
        }

        public static ProductTypeCustomizationOptionModel ToOptionModel(ProductTypeCustomizationOption row)
        {
            return new ProductTypeCustomizationOptionModel
            {
                OptionId = row.OptionId,
                GroupId = row.GroupId,
                Label = row.Label,
                PriceDeltaCents = row.PriceDeltaCents,
                IsDefault = row.IsDefault,
                DisplayOrder = row.DisplayOrder
            };
        }
    }
}
