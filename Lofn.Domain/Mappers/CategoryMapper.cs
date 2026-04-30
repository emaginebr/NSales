using Lofn.Domain.Models;
using Lofn.DTO.Category;

namespace Lofn.Domain.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryInfo ToInfo(CategoryModel md)
        {
            return ToInfo(md, null, null);
        }

        public static CategoryInfo ToInfo(CategoryModel md, long? appliedProductTypeId, long? appliedProductTypeOriginCategoryId)
        {
            return new CategoryInfo
            {
                CategoryId = md.CategoryId,
                Slug = md.Slug,
                Name = md.Name,
                StoreId = md.StoreId,
                IsGlobal = md.StoreId == null,
                ParentCategoryId = md.ParentId,
                ProductTypeId = md.ProductTypeId,
                AppliedProductTypeId = appliedProductTypeId,
                AppliedProductTypeOriginCategoryId = appliedProductTypeOriginCategoryId
            };
        }

        public static CategoryModel ToModel(CategoryInfo dto)
        {
            return new CategoryModel
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                StoreId = dto.StoreId,
                ParentId = dto.ParentCategoryId,
                ProductTypeId = dto.ProductTypeId
            };
        }
    }
}
