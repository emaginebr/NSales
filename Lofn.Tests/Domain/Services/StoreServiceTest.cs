using Xunit;
using Moq;
using Lofn.Domain.Core;
using Lofn.Domain.Services;
using Lofn.Domain.Models;
using Lofn.DTO.Store;
using Lofn.Infra.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Tests.Domain.Services
{
    public class StoreServiceTest
    {
        private readonly Mock<ISlugGenerator> _slugGeneratorMock;
        private readonly Mock<IStoreRepository<StoreModel>> _storeRepositoryMock;
        private readonly Mock<IStoreUserRepository<StoreUserModel>> _storeUserRepositoryMock;
        private readonly StoreService _sut;

        public StoreServiceTest()
        {
            _slugGeneratorMock = new Mock<ISlugGenerator>();
            _storeRepositoryMock = new Mock<IStoreRepository<StoreModel>>();
            _storeUserRepositoryMock = new Mock<IStoreUserRepository<StoreUserModel>>();
            _sut = new StoreService(
                _slugGeneratorMock.Object,
                _storeRepositoryMock.Object,
                _storeUserRepositoryMock.Object
            );
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            var store = new StoreInsertInfo { Name = "" };

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.InsertAsync(store, 1));
            Assert.Equal("Name is required", ex.Message);
        }

        [Fact]
        public async Task InsertAsync_ShouldCreateStoreAndAddOwnerToStoreUsers()
        {
            var store = new StoreInsertInfo { Name = "Minha Loja" };
            var createdModel = new StoreModel { StoreId = 10, Name = "Minha Loja", OwnerId = 1, Slug = "minha-loja" };

            _slugGeneratorMock.Setup(x => x.Generate("Minha Loja")).Returns("minha-loja");
            _storeRepositoryMock.Setup(x => x.ExistSlugAsync(0, "minha-loja")).ReturnsAsync(false);
            _storeRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<StoreModel>())).ReturnsAsync(createdModel);
            _storeUserRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<StoreUserModel>())).ReturnsAsync(new StoreUserModel());

            var result = await _sut.InsertAsync(store, 1);

            Assert.Equal(10, result.StoreId);
            _storeUserRepositoryMock.Verify(x => x.InsertAsync(It.Is<StoreUserModel>(
                su => su.StoreId == 10 && su.UserId == 1)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            var store = new StoreUpdateInfo { StoreId = 1, Name = "" };

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(store, 1));
            Assert.Equal("Name is required", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenStoreNotFound()
        {
            var store = new StoreUpdateInfo { StoreId = 99, Name = "Nova" };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((StoreModel)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(store, 1));
            Assert.Equal("Store not found", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorized_WhenUserIsNotOwner()
        {
            var store = new StoreUpdateInfo { StoreId = 1, Name = "Nova" };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new StoreModel { StoreId = 1, OwnerId = 5 });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(store, 999));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateStore_WhenValid()
        {
            var store = new StoreUpdateInfo { StoreId = 1, Name = "Atualizada" };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new StoreModel { StoreId = 1, OwnerId = 1 });
            _slugGeneratorMock.Setup(x => x.Generate("Atualizada")).Returns("atualizada");
            _storeRepositoryMock.Setup(x => x.ExistSlugAsync(1, "atualizada")).ReturnsAsync(false);
            _storeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<StoreModel>())).ReturnsAsync(new StoreModel { StoreId = 1, Name = "Atualizada", Slug = "atualizada" });

            var result = await _sut.UpdateAsync(store, 1);

            Assert.Equal("Atualizada", result.Name);
        }

        [Fact]
        public async Task ListByOwnerAsync_ShouldReturnStoresForOwner()
        {
            var models = new List<StoreModel>
            {
                new StoreModel { StoreId = 1, Name = "Loja 1", Slug = "loja-1", OwnerId = 1 },
                new StoreModel { StoreId = 2, Name = "Loja 2", Slug = "loja-2", OwnerId = 1 }
            };
            _storeRepositoryMock.Setup(x => x.ListByOwnerAsync(1)).ReturnsAsync(models);

            var result = await _sut.ListByOwnerAsync(1);

            Assert.Equal(2, result.Count);
        }
    }
}
