using Core.Domain.Repository;
using Core.Domain;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using NSales.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Impl.Models
{
    public class ProductModel : IProductModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository<IProductModel, IProductDomainFactory> _repositoryProduct;

        public ProductModel(IUnitOfWork unitOfWork, IProductRepository<IProductModel, IProductDomainFactory> repositoryProduct)
        {
            _unitOfWork = unitOfWork;
            _repositoryProduct = repositoryProduct;
        }

        public long ProductId { get; set; }
        public long? NetworkId { get; set; }
        public long UserId { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Frequency { get; set; }
        public int Limit { get; set; }
        public ProductStatusEnum Status { get; set; }
        public string StripeProductId { get; set; }
        public string StripePriceId { get; set; }

        public IProductModel GetById(long id, IProductDomainFactory factory)
        {
            return _repositoryProduct.GetById(id, factory);
        }

        public IProductModel GetBySlug(string slug, IProductDomainFactory factory)
        {
            return _repositoryProduct.GetBySlug(slug, factory);
        }

        public IProductModel Insert(IProductDomainFactory factory)
        {
            return _repositoryProduct.Insert(this, factory);
        }

        public IProductModel Update(IProductDomainFactory factory)
        {
            return _repositoryProduct.Update(this, factory);
        }

        public IEnumerable<IProductModel> Search(long? networkId, long? userId, string keyword, bool active, int pageNum, out int pageCount, IProductDomainFactory factory)
        {
            return _repositoryProduct.Search(networkId, userId, keyword, active, pageNum, out pageCount, factory);
        }

        public IEnumerable<IProductModel> ListByNetwork(long networkId, IProductDomainFactory factory)
        {
            return _repositoryProduct.ListByNetwork(networkId, factory);
        }

        public bool ExistSlug(long productId, string slug)
        {
            return _repositoryProduct.ExistSlug(productId, slug);
        }

        /*
        public IProductModel GetByStripeProductId(string stripeProductId, IProductDomainFactory factory)
        {
            return _repositoryProduct.GetByStripeProductId(stripeProductId, factory);
        }

        public IProductModel GetByStripePriceId(string stripePriceId, IProductDomainFactory factory)
        {
            return _repositoryProduct.GetByStripeProductId(stripePriceId, factory);
        }
        */
    }
}
