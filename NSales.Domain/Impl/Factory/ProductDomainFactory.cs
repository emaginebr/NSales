using Core.Domain.Repository;
using Core.Domain;
using NSales.Domain.Interfaces.Factory;
using NSales.Domain.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSales.Domain.Impl.Models;

namespace NSales.Domain.Impl.Factory
{
    public class ProductDomainFactory : IProductDomainFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository<IProductModel, IProductDomainFactory> _repositoryProduct;

        public ProductDomainFactory(IUnitOfWork unitOfWork, IProductRepository<IProductModel, IProductDomainFactory> repositoryProduct)
        {
            _unitOfWork = unitOfWork;
            _repositoryProduct = repositoryProduct;
        }

        public IProductModel BuildProductModel()
        {
            return new ProductModel(_unitOfWork, _repositoryProduct);
        }
    }
}
