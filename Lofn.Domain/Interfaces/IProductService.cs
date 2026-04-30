using Lofn.Domain.Models;
using Lofn.DTO.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Domain.Interfaces
{
    public interface IProductService
    {
        Task<ProductListPagedResult> SearchAsync(ProductSearchInternalParam param);
        Task<IList<ProductModel>> ListByStoreAsync(long storeId);
        Task<ProductModel> GetByIdAsync(long productId);
        Task<ProductModel> GetByIdAsync(long productId, long storeId, long userId);
        Task<ProductModel> GetBySlugAsync(string productSlug);
        Task<ProductInfo> GetProductInfoAsync(ProductModel product);
        Task<ProductListPagedResult> ListActiveByStoreSlugAsync(string storeSlug, string categorySlug, int pageNum);
        Task<IList<ProductInfo>> ListFeaturedByStoreSlugAsync(string storeSlug, int limit);
        Task<ProductModel> InsertAsync(ProductInsertInfo product, long storeId, long userId);
        Task<ProductModel> UpdateAsync(ProductUpdateInfo product, long storeId, long userId);

        // 003-product-type-filters
        Task<ProductSearchFilteredResult> SearchFilteredAsync(ProductSearchFilteredParam param);
    }
}
