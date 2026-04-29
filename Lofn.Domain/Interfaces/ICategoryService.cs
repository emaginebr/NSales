using Lofn.Domain.Models;
using Lofn.DTO.Category;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Domain.Interfaces
{
    public interface ICategoryService
    {
        Task<IList<CategoryInfo>> ListAllAsync();
        Task<IList<CategoryInfo>> ListByStoreAsync(long storeId);
        Task<IList<CategoryInfo>> ListWithProductCountAsync();
        Task<IList<CategoryInfo>> ListActiveByStoreSlugAsync(string storeSlug);
        Task<CategoryInfo> GetBySlugAndStoreSlugAsync(string storeSlug, string categorySlug);
        Task<CategoryModel> GetByIdAsync(long categoryId, long storeId, long userId);
        Task<CategoryModel> InsertAsync(CategoryInsertInfo category, long storeId, long userId);
        Task<CategoryModel> UpdateAsync(CategoryUpdateInfo category, long storeId, long userId);
        Task DeleteAsync(long categoryId, long storeId, long userId);

        Task<CategoryInfo> InsertGlobalAsync(CategoryGlobalInsertInfo category);
        Task<CategoryInfo> UpdateGlobalAsync(CategoryGlobalUpdateInfo category);
        Task DeleteGlobalAsync(long categoryId);
        Task<IList<CategoryInfo>> ListGlobalAsync();

        // 002-category-subcategories
        Task<IList<CategoryTreeNodeInfo>> GetTreeAsync(long? storeId);
    }
}
