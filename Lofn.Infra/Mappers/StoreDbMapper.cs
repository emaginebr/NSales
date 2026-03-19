using Lofn.Domain.Models;
using Lofn.DTO.Store;
using Lofn.Infra.Context;

namespace Lofn.Infra.Mappers
{
    public static class StoreDbMapper
    {
        public static StoreModel ToModel(Store row)
        {
            return new StoreModel
            {
                StoreId = row.StoreId,
                Slug = row.Slug,
                Name = row.Name,
                OwnerId = row.OwnerId,
                Logo = row.Logo,
                Status = (StoreStatusEnum)row.Status
            };
        }

        public static void ToEntity(StoreModel md, Store row)
        {
            row.StoreId = md.StoreId;
            row.Slug = md.Slug;
            row.Name = md.Name;
            row.OwnerId = md.OwnerId;
            row.Logo = md.Logo;
            row.Status = (int)md.Status;
        }
    }
}
