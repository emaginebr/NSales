using Lofn.Domain.Models;
using Lofn.Infra.Context;

namespace Lofn.Infra.Mappers
{
    public static class CategoryDbMapper
    {
        public static CategoryModel ToModel(Category row)
        {
            return new CategoryModel
            {
                CategoryId = row.CategoryId,
                Slug = row.Slug,
                Name = row.Name,
                StoreId = row.StoreId
            };
        }

        public static void ToEntity(CategoryModel md, Category row)
        {
            row.CategoryId = md.CategoryId;
            row.Slug = md.Slug;
            row.Name = md.Name;
            row.StoreId = md.StoreId;
        }
    }
}
