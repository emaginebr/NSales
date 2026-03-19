using Lofn.DTO.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.ACL.Interfaces
{
    public interface IProductClient
    {
        Task<ProductListPagedInfo> SearchAsync(ProductSearchParam param);
        Task<ProductInfo> GetByIdAsync(string storeSlug, long productId);
        Task<ProductInfo> GetBySlugAsync(string productSlug);
        Task<IList<ProductInfo>> ListActiveByCategoryAsync(string storeSlug, string categorySlug);
        Task<ProductInfo> InsertAsync(string storeSlug, ProductInsertInfo product);
        Task<ProductInfo> UpdateAsync(string storeSlug, ProductUpdateInfo product);
    }
}
