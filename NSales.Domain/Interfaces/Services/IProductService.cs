using NSales.Domain.Interfaces.Models;
using NSales.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Interfaces.Services
{
    public interface IProductService
    {
        ProductListPagedResult Search(ProductSearchInternalParam param);
        IList<IProductModel> ListByNetwork(long networkId);
        IProductModel GetById(long productId);
        IProductModel GetBySlug(string productSlug);
        /*
        IProductModel GetByStripeProductId(string stripeProductId);
        IProductModel GetByStripePriceId(string stripePriceId);
        */
        Task<ProductInfo> GetProductInfo(IProductModel product);
        Task<IProductModel> Insert(ProductInfo product, long userId);
        Task<IProductModel> Update(ProductInfo product, long userId);
    }
}
