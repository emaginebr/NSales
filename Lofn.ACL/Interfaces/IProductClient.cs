using Lofn.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lofn.ACL.Interfaces
{
    public interface IProductClient
    {
        Task<ProductListPagedInfo> SearchAsync(ProductSearchParam param);
        Task<ProductInfo> GetByIdAsync(long productId);
        Task<ProductInfo> GetBySlugAsync(string productSlug);
        Task<ProductInfo> InsertAsync(ProductInfo product);
        Task<ProductInfo> UpdateAsync(ProductInfo product);
    }
}
