using Lofn.DTO.Category;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.ACL.Interfaces
{
    public interface ICategoryClient
    {
        Task<IList<CategoryInfo>> ListAsync(string storeSlug);
        Task<IList<CategoryInfo>> ListActiveAsync(string storeSlug);
        Task<CategoryInfo> GetBySlugAsync(string storeSlug, string categorySlug);
        Task<CategoryInfo> GetByIdAsync(string storeSlug, long categoryId);
        Task<CategoryInfo> InsertAsync(string storeSlug, CategoryInsertInfo category);
        Task<CategoryInfo> UpdateAsync(string storeSlug, CategoryUpdateInfo category);
        Task DeleteAsync(string storeSlug, long categoryId);
    }
}
