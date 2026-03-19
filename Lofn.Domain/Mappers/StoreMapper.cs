using Lofn.Domain.Models;
using Lofn.DTO.Store;

namespace Lofn.Domain.Mappers
{
    public static class StoreMapper
    {
        public static StoreInfo ToInfo(StoreModel md)
        {
            return new StoreInfo
            {
                StoreId = md.StoreId,
                Slug = md.Slug,
                Name = md.Name,
                OwnerId = md.OwnerId,
                Logo = md.Logo,
                Status = md.Status
            };
        }

        public static StoreModel ToInsertModel(StoreInsertInfo dto, long ownerId)
        {
            return new StoreModel
            {
                Name = dto.Name,
                OwnerId = ownerId,
                Status = StoreStatusEnum.Active
            };
        }

        public static StoreModel ToUpdateModel(StoreUpdateInfo dto, long ownerId)
        {
            return new StoreModel
            {
                StoreId = dto.StoreId,
                Name = dto.Name,
                OwnerId = ownerId,
                Status = dto.Status
            };
        }
    }
}
