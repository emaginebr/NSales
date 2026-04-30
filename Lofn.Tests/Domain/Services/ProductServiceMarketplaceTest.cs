using FluentValidation;
using Lofn.Domain.Core;
using Lofn.Domain.Interfaces;
using Lofn.Domain.Models;
using Lofn.Domain.Services;
using Lofn.DTO.Product;
using Lofn.Infra.Interfaces.Repository;
using Moq;
using System.Threading.Tasks;
using Xunit;
using zTools.ACL.Interfaces;

namespace Lofn.Tests.Domain.Services
{
    public class ProductServiceMarketplaceTest
    {
        private readonly Mock<ITenantResolver> _tenantResolverMock = new();
        private readonly Mock<IFileClient> _fileClientMock = new();
        private readonly Mock<ISlugGenerator> _slugGeneratorMock = new();
        private readonly Mock<IProductRepository<ProductModel>> _productRepositoryMock = new();
        private readonly Mock<IProductImageService> _productImageServiceMock = new();
        private readonly Mock<IStoreUserRepository<StoreUserModel>> _storeUserRepositoryMock = new();
        private readonly Mock<IStoreRepository<StoreModel>> _storeRepositoryMock = new();
        private readonly Mock<ICategoryRepository<CategoryModel>> _categoryRepositoryMock = new();
        private readonly Mock<ICategoryService> _categoryServiceMock = new();
        private readonly Mock<IProductFilterValueRepository<ProductFilterValueModel>> _filterValueRepositoryMock = new();

        private ProductService BuildSut(bool marketplace)
        {
            _tenantResolverMock.Setup(x => x.Marketplace).Returns(marketplace);
            _filterValueRepositoryMock
                .Setup(r => r.GetByProductAsync(It.IsAny<long>()))
                .ReturnsAsync(new System.Collections.Generic.List<ProductFilterValueModel>());
            return new ProductService(
                _tenantResolverMock.Object,
                _fileClientMock.Object,
                _slugGeneratorMock.Object,
                _productRepositoryMock.Object,
                _productImageServiceMock.Object,
                _storeUserRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                new ProductFilterValueResolver(_categoryServiceMock.Object),
                _filterValueRepositoryMock.Object
            );
        }

        private void GivenStoreUserExists(long storeId, long userId)
        {
            _storeUserRepositoryMock.Setup(x => x.ExistsAsync(storeId, userId)).ReturnsAsync(true);
        }

        private void GivenSlugFor(string name, string slug)
        {
            _slugGeneratorMock.Setup(x => x.Generate(name)).Returns(slug);
            _productRepositoryMock.Setup(x => x.ExistSlugAsync(It.IsAny<long>(), It.IsAny<long>(), slug)).ReturnsAsync(false);
        }

        [Fact]
        public async Task InsertAsync_OnMarketplace_WithGlobalCategory_ShouldSucceed()
        {
            var sut = BuildSut(marketplace: true);
            GivenStoreUserExists(1, 1);
            GivenSlugFor("Mouse", "mouse");
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(50))
                .ReturnsAsync(new CategoryModel { CategoryId = 50, StoreId = null });
            _productRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ProductModel>()))
                .ReturnsAsync((ProductModel m) => { m.ProductId = 100; return m; });

            var product = new ProductInsertInfo { Name = "Mouse", Price = 10, CategoryId = 50 };
            var result = await sut.InsertAsync(product, 1, 1);

            Assert.Equal(100, result.ProductId);
            Assert.Equal((long?)50, result.CategoryId);
        }

        [Fact]
        public async Task InsertAsync_OnMarketplace_WithStoreScopedCategory_ShouldThrow()
        {
            var sut = BuildSut(marketplace: true);
            GivenStoreUserExists(1, 1);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(50))
                .ReturnsAsync(new CategoryModel { CategoryId = 50, StoreId = 1 });

            var product = new ProductInsertInfo { Name = "Mouse", Price = 10, CategoryId = 50 };
            var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.InsertAsync(product, 1, 1));
            Assert.Contains("tenant-global category in marketplace mode", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_OffMarketplace_WithSameStoreCategory_ShouldSucceed()
        {
            var sut = BuildSut(marketplace: false);
            GivenStoreUserExists(1, 1);
            GivenSlugFor("Mouse", "mouse");
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(50))
                .ReturnsAsync(new CategoryModel { CategoryId = 50, StoreId = 1 });
            _productRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ProductModel>()))
                .ReturnsAsync((ProductModel m) => { m.ProductId = 100; return m; });

            var product = new ProductInsertInfo { Name = "Mouse", Price = 10, CategoryId = 50 };
            var result = await sut.InsertAsync(product, 1, 1);

            Assert.Equal(100, result.ProductId);
        }

        [Fact]
        public async Task InsertAsync_OffMarketplace_WithCrossStoreCategory_ShouldThrow()
        {
            var sut = BuildSut(marketplace: false);
            GivenStoreUserExists(1, 1);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(50))
                .ReturnsAsync(new CategoryModel { CategoryId = 50, StoreId = 999 });

            var product = new ProductInsertInfo { Name = "Mouse", Price = 10, CategoryId = 50 };
            var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.InsertAsync(product, 1, 1));
            Assert.Contains("does not belong to this store", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_OffMarketplace_WithGlobalCategory_ShouldThrow()
        {
            var sut = BuildSut(marketplace: false);
            GivenStoreUserExists(1, 1);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(50))
                .ReturnsAsync(new CategoryModel { CategoryId = 50, StoreId = null });

            var product = new ProductInsertInfo { Name = "Mouse", Price = 10, CategoryId = 50 };
            var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.InsertAsync(product, 1, 1));
            Assert.Contains("does not belong to this store", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_WithNullCategory_ShouldSucceed_InEitherMode()
        {
            foreach (var mode in new[] { true, false })
            {
                var sut = BuildSut(marketplace: mode);
                GivenStoreUserExists(1, 1);
                GivenSlugFor("Mouse", "mouse");
                _productRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ProductModel>()))
                    .ReturnsAsync((ProductModel m) => { m.ProductId = 100; return m; });

                var product = new ProductInsertInfo { Name = "Mouse", Price = 10, CategoryId = null };
                var result = await sut.InsertAsync(product, 1, 1);

                Assert.Null(result.CategoryId);
            }
        }

        [Fact]
        public async Task InsertAsync_WithUnknownCategory_ShouldThrow()
        {
            var sut = BuildSut(marketplace: true);
            GivenStoreUserExists(1, 1);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((CategoryModel)null);

            var product = new ProductInsertInfo { Name = "Mouse", Price = 10, CategoryId = 999 };
            var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.InsertAsync(product, 1, 1));
            Assert.Contains("not found", ex.Message);
        }
    }
}
