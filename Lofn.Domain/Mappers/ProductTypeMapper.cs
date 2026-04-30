using System.Collections.Generic;
using System.Linq;
using Lofn.Domain.Models;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Mappers
{
    public static class ProductTypeMapper
    {
        public static ProductTypeInfo ToInfo(ProductTypeModel md)
        {
            if (md == null) return null;
            return new ProductTypeInfo
            {
                ProductTypeId = md.ProductTypeId,
                Name = md.Name,
                Description = md.Description,
                Filters = (md.Filters ?? new List<ProductTypeFilterModel>())
                    .Select(ToFilterInfo)
                    .ToList(),
                CustomizationGroups = (md.CustomizationGroups ?? new List<ProductTypeCustomizationGroupModel>())
                    .Select(ToCustomizationGroupInfo)
                    .ToList(),
                CreatedAt = md.CreatedAt,
                UpdatedAt = md.UpdatedAt
            };
        }

        public static ProductTypeFilterInfo ToFilterInfo(ProductTypeFilterModel md)
        {
            if (md == null) return null;
            return new ProductTypeFilterInfo
            {
                FilterId = md.FilterId,
                ProductTypeId = md.ProductTypeId,
                Label = md.Label,
                DataType = md.DataType,
                IsRequired = md.IsRequired,
                DisplayOrder = md.DisplayOrder,
                AllowedValues = (md.AllowedValues ?? new List<string>()).ToList()
            };
        }

        public static ProductTypeModel ToInsertModel(ProductTypeInsertInfo dto)
        {
            return new ProductTypeModel
            {
                Name = dto.Name,
                Description = dto.Description
            };
        }

        public static ProductTypeModel ToUpdateModel(ProductTypeUpdateInfo dto, ProductTypeModel existing)
        {
            if (existing == null) return null;
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            return existing;
        }

        public static ProductTypeFilterModel ToFilterInsertModel(long productTypeId, ProductTypeFilterInsertInfo dto)
        {
            return new ProductTypeFilterModel
            {
                ProductTypeId = productTypeId,
                Label = dto.Label,
                DataType = dto.DataType,
                IsRequired = dto.IsRequired,
                DisplayOrder = dto.DisplayOrder,
                AllowedValues = (dto.AllowedValues ?? new List<string>()).ToList()
            };
        }

        public static ProductTypeFilterModel ToFilterUpdateModel(ProductTypeFilterUpdateInfo dto, ProductTypeFilterModel existing)
        {
            if (existing == null) return null;
            existing.Label = dto.Label;
            existing.IsRequired = dto.IsRequired;
            existing.DisplayOrder = dto.DisplayOrder;
            if (dto.AllowedValues != null)
                existing.AllowedValues = dto.AllowedValues.ToList();
            return existing;
        }

        public static CustomizationGroupInfo ToCustomizationGroupInfo(ProductTypeCustomizationGroupModel md)
        {
            if (md == null) return null;
            return new CustomizationGroupInfo
            {
                GroupId = md.GroupId,
                ProductTypeId = md.ProductTypeId,
                Label = md.Label,
                SelectionMode = md.SelectionMode,
                IsRequired = md.IsRequired,
                DisplayOrder = md.DisplayOrder,
                Options = (md.Options ?? new List<ProductTypeCustomizationOptionModel>())
                    .Select(ToCustomizationOptionInfo)
                    .ToList()
            };
        }

        public static CustomizationOptionInfo ToCustomizationOptionInfo(ProductTypeCustomizationOptionModel md)
        {
            if (md == null) return null;
            return new CustomizationOptionInfo
            {
                OptionId = md.OptionId,
                GroupId = md.GroupId,
                Label = md.Label,
                PriceDeltaCents = md.PriceDeltaCents,
                IsDefault = md.IsDefault,
                DisplayOrder = md.DisplayOrder
            };
        }

        public static ProductTypeCustomizationGroupModel ToInsertGroupModel(long productTypeId, CustomizationGroupInsertInfo dto)
        {
            return new ProductTypeCustomizationGroupModel
            {
                ProductTypeId = productTypeId,
                Label = dto.Label,
                SelectionMode = dto.SelectionMode,
                IsRequired = dto.IsRequired,
                DisplayOrder = dto.DisplayOrder
            };
        }

        public static ProductTypeCustomizationGroupModel ToUpdateGroupModel(CustomizationGroupUpdateInfo dto, ProductTypeCustomizationGroupModel existing)
        {
            if (existing == null) return null;
            existing.Label = dto.Label;
            existing.SelectionMode = dto.SelectionMode;
            existing.IsRequired = dto.IsRequired;
            existing.DisplayOrder = dto.DisplayOrder;
            return existing;
        }

        public static ProductTypeCustomizationOptionModel ToInsertOptionModel(long groupId, CustomizationOptionInsertInfo dto)
        {
            return new ProductTypeCustomizationOptionModel
            {
                GroupId = groupId,
                Label = dto.Label,
                PriceDeltaCents = dto.PriceDeltaCents,
                IsDefault = dto.IsDefault,
                DisplayOrder = dto.DisplayOrder
            };
        }

        public static ProductTypeCustomizationOptionModel ToUpdateOptionModel(CustomizationOptionUpdateInfo dto, ProductTypeCustomizationOptionModel existing)
        {
            if (existing == null) return null;
            existing.Label = dto.Label;
            existing.PriceDeltaCents = dto.PriceDeltaCents;
            existing.IsDefault = dto.IsDefault;
            existing.DisplayOrder = dto.DisplayOrder;
            return existing;
        }
    }
}
