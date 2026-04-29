using Xunit;
using Moq;
using Lofn.Domain.Core;
using Lofn.Domain.Services;
using Lofn.Domain.Models;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Product;
using Lofn.Infra.Interfaces.Repository;
using zTools.ACL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lofn.Tests.Domain.Services
{
    public class ProductServiceTest
    {
        private readonly Mock<ITenantResolver> _tenantResolverMock;
        private readonly Mock<IFileClient> _fileClientMock;
        private readonly Mock<ISlugGenerator> _slugGeneratorMock;
        private readonly Mock<IProductRepository<ProductModel>> _productRepositoryMock;
        private readonly Mock<IProductImageService> _productImageServiceMock;
        private readonly Mock<IStoreUserRepository<StoreUserModel>> _storeUserRepositoryMock;
        private readonly Mock<IStoreRepository<StoreModel>> _storeRepositoryMock;
        private readonly Mock<ICategoryRepository<CategoryModel>> _categoryRepositoryMock;
        private readonly ProductService _sut;

        public ProductServiceTest()
        {
            _tenantResolverMock = new Mock<ITenantResolver>();
            _fileClientMock = new Mock<IFileClient>();
            _slugGeneratorMock = new Mock<ISlugGenerator>();
            _productRepositoryMock = new Mock<IProductRepository<ProductModel>>();
            _productImageServiceMock = new Mock<IProductImageService>();
            _storeUserRepositoryMock = new Mock<IStoreUserRepository<StoreUserModel>>();
            _storeRepositoryMock = new Mock<IStoreRepository<StoreModel>>();
            _categoryRepositoryMock = new Mock<ICategoryRepository<CategoryModel>>();
            _sut = new ProductService(
                _tenantResolverMock.Object,
                _fileClientMock.Object,
                _slugGeneratorMock.Object,
                _productRepositoryMock.Object,
                _productImageServiceMock.Object,
                _storeUserRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _categoryRepositoryMock.Object
            );
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            var product = new ProductInsertInfo { Name = "", Price = 10 };

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.InsertAsync(product, 1, 1));
            Assert.Equal("Name is required", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenPriceIsZero()
        {
            var product = new ProductInsertInfo { Name = "Produto", Price = 0 };

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.InsertAsync(product, 1, 1));
            Assert.Equal("Price is required", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowUnauthorized_WhenUserNotInStore()
        {
            var product = new ProductInsertInfo { Name = "Produto", Price = 10 };
            _storeUserRepositoryMock.Setup(x => x.ExistsAsync(1, 1)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.InsertAsync(product, 1, 1));
        }

        [Fact]
        public async Task InsertAsync_ShouldCreateProduct_WhenValid()
        {
            var product = new ProductInsertInfo { Name = "Produto", Price = 99.90, Description = "Desc" };
            _storeUserRepositoryMock.Setup(x => x.ExistsAsync(1, 1)).ReturnsAsync(true);
            _slugGeneratorMock.Setup(x => x.Generate("Produto")).Returns("produto");
            _productRepositoryMock.Setup(x => x.ExistSlugAsync(1, 0, "produto")).ReturnsAsync(false);
            _productRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ProductModel>()))
                .ReturnsAsync(new ProductModel { ProductId = 1, Name = "Produto", StoreId = 1, Slug = "produto", Price = 99.90 });

            var result = await _sut.InsertAsync(product, 1, 1);

            Assert.Equal("Produto", result.Name);
            Assert.Equal(1, result.StoreId);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenProductNotFound()
        {
            var product = new ProductUpdateInfo { ProductId = 99, Name = "Novo", Price = 10 };
            _storeUserRepositoryMock.Setup(x => x.ExistsAsync(1, 1)).ReturnsAsync(true);
            _productRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((ProductModel)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(product, 1, 1));
            Assert.Equal("Product not found", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorized_WhenProductBelongsToDifferentStore()
        {
            var product = new ProductUpdateInfo { ProductId = 1, Name = "Novo", Price = 10 };
            _storeUserRepositoryMock.Setup(x => x.ExistsAsync(1, 1)).ReturnsAsync(true);
            _productRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new ProductModel { ProductId = 1, StoreId = 999 });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(product, 1, 1));
        }

        [Fact]
        public async Task GetByIdAsync_WithStoreValidation_ShouldThrowUnauthorized_WhenUserNotInStore()
        {
            _storeUserRepositoryMock.Setup(x => x.ExistsAsync(1, 1)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetByIdAsync(1, 1, 1));
        }

        [Fact]
        public async Task GetByIdAsync_WithStoreValidation_ShouldReturnNull_WhenProductNotFound()
        {
            _storeUserRepositoryMock.Setup(x => x.ExistsAsync(1, 1)).ReturnsAsync(true);
            _productRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((ProductModel)null);

            var result = await _sut.GetByIdAsync(99, 1, 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithoutValidation_ShouldReturnProduct()
        {
            var model = new ProductModel { ProductId = 1, Name = "P" };
            _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(model);

            var result = await _sut.GetByIdAsync(1);

            Assert.Equal("P", result.Name);
        }
    }
}
