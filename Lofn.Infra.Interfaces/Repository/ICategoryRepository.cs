using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces.Repository
{
    public interface ICategoryRepository<TModel> where TModel : class
    {
        Task<IEnumerable<TModel>> ListAllAsync();
        Task<IEnumerable<TModel>> ListByStoreAsync(long storeId);
        Task<IEnumerable<TModel>> ListGlobalAsync();
        Task<TModel> GetByIdAsync(long id);
        Task<TModel> GetBySlugAsync(string slug);
        Task<TModel> InsertAsync(TModel model);
        Task<TModel> UpdateAsync(TModel model);
        Task DeleteAsync(long id);
        Task<bool> ExistSlugAsync(long storeId, long categoryId, string slug);
        Task<bool> ExistSlugInTenantAsync(long? exceptCategoryId, string slug);
        Task<IDictionary<long, int>> CountProductsByCategoryAsync();
        Task<IDictionary<long, int>> CountActiveProductsByStoreAsync(long storeId);
        Task<TModel> GetBySlugAndStoreAsync(long storeId, string slug);

        // 002-category-subcategories
        Task<IList<TModel>> GetAncestorChainAsync(long categoryId);
        Task<bool> ExistSiblingNameAsync(long? parentId, long? storeId, string name, long? excludeCategoryId);
        Task<bool> HasChildrenAsync(long categoryId);
        Task<IList<TModel>> ListByScopeAsync(long? storeId);
        Task<IList<TModel>> GetDescendantsAsync(long categoryId);
        Task UpdateManyAsync(IEnumerable<TModel> models);

        // 003-product-type-filters
        Task<(long? AppliedProductTypeId, long? OriginCategoryId)> GetAppliedProductTypeAsync(long categoryId);
        Task UpdateProductTypeIdAsync(long categoryId, long? productTypeId);
    }
}
