using Lofn.Infra.Interfaces.Repository;
using Lofn.Domain.Core;
using Lofn.Domain.Mappers;
using Lofn.Domain.Models;
using Lofn.Domain.Interfaces;
using Lofn.DTO.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lofn.Domain.Services
{
    public class StoreService : IStoreService
    {
        private readonly ISlugGenerator _slugGenerator;
        private readonly IStoreRepository<StoreModel> _storeRepository;
        private readonly IStoreUserRepository<StoreUserModel> _storeUserRepository;

        public StoreService(
            ISlugGenerator slugGenerator,
            IStoreRepository<StoreModel> storeRepository,
            IStoreUserRepository<StoreUserModel> storeUserRepository
        )
        {
            _slugGenerator = slugGenerator;
            _storeRepository = storeRepository;
            _storeUserRepository = storeUserRepository;
        }

        public async Task<IList<StoreInfo>> ListAllAsync()
        {
            var items = await _storeRepository.ListAllAsync();
            return items.Select(StoreMapper.ToInfo).ToList();
        }

        public async Task<IList<StoreInfo>> ListActiveAsync()
        {
            var items = await _storeRepository.ListActiveAsync();
            return items.Select(StoreMapper.ToInfo).ToList();
        }

        public async Task<IList<StoreInfo>> ListByOwnerAsync(long ownerId)
        {
            var items = await _storeRepository.ListByOwnerAsync(ownerId);
            return items.Select(StoreMapper.ToInfo).ToList();
        }

        public async Task<StoreModel> GetByIdAsync(long storeId)
        {
            return await _storeRepository.GetByIdAsync(storeId);
        }

        public async Task<StoreModel> GetBySlugAsync(string slug)
        {
            return await _storeRepository.GetBySlugAsync(slug);
        }

        private async Task<string> GenerateSlugAsync(long storeId, string name)
        {
            var baseSlug = _slugGenerator.Generate(name);
            string newSlug;
            int c = 0;
            do
            {
                newSlug = baseSlug;
                if (c > 0)
                {
                    newSlug += c.ToString();
                }
                c++;
            } while (await _storeRepository.ExistSlugAsync(storeId, newSlug));
            return newSlug;
        }

        public async Task<StoreModel> InsertAsync(StoreInsertInfo store, long ownerId)
        {
            if (string.IsNullOrEmpty(store.Name))
                throw new Exception("Name is required");

            var model = StoreMapper.ToInsertModel(store, ownerId);
            model.Slug = await GenerateSlugAsync(0, store.Name);
            var created = await _storeRepository.InsertAsync(model);

            await _storeUserRepository.InsertAsync(new StoreUserModel
            {
                StoreId = created.StoreId,
                UserId = ownerId
            });

            return created;
        }

        public async Task<StoreModel> UpdateAsync(StoreUpdateInfo store, long ownerId)
        {
            if (string.IsNullOrEmpty(store.Name))
                throw new Exception("Name is required");

            var existing = await _storeRepository.GetByIdAsync(store.StoreId);
            if (existing == null)
                throw new Exception("Store not found");

            if (existing.OwnerId != ownerId)
                throw new UnauthorizedAccessException("Access denied: user is not the owner of this store");

            var model = StoreMapper.ToUpdateModel(store, ownerId);
            model.Slug = await GenerateSlugAsync(store.StoreId, store.Name);
            return await _storeRepository.UpdateAsync(model);
        }

        public async Task<StoreModel> UploadLogoAsync(long storeId, string fileName, long ownerId)
        {
            var existing = await _storeRepository.GetByIdAsync(storeId);
            if (existing == null)
                throw new Exception("Store not found");

            if (existing.OwnerId != ownerId)
                throw new UnauthorizedAccessException("Access denied: user is not the owner of this store");

            existing.Logo = fileName;
            return await _storeRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(long storeId)
        {
            await _storeRepository.DeleteAsync(storeId);
        }
    }
}
