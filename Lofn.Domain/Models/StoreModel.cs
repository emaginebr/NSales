using Lofn.DTO.Store;

namespace Lofn.Domain.Models
{
    public class StoreModel
    {
        public long StoreId { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public long OwnerId { get; set; }
        public string Logo { get; set; }
        public StoreStatusEnum Status { get; set; }
    }
}
