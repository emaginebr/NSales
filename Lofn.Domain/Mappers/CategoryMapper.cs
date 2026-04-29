using Lofn.Domain.Models;
using Lofn.DTO.Category;

namespace Lofn.Domain.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryInfo ToInfo(CategoryModel md)
        {
            return new CategoryInfo
            {
                CategoryId = md.CategoryId,
                Slug = md.Slug,
                Name = md.Name,
                StoreId = md.StoreId,
                IsGlobal = md.StoreId == null,
                ParentCategoryId = md.ParentId
            };
        }

        public static CategoryModel ToModel(CategoryInfo dto)
        {
            return new CategoryModel
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                StoreId = dto.StoreId,
                ParentId = dto.ParentCategoryId
            };
        }
    }
}
