using System.Text.Json.Serialization;

namespace Lofn.DTO.Store
{
    public class StoreUpdateInfo
    {
        [JsonPropertyName("storeId")]
        public long StoreId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("status")]
        public StoreStatusEnum Status { get; set; }
    }
}
