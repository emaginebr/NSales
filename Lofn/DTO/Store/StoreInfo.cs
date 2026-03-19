using System.Text.Json.Serialization;

namespace Lofn.DTO.Store
{
    public class StoreInfo
    {
        [JsonPropertyName("storeId")]
        public long StoreId { get; set; }
        [JsonPropertyName("slug")]
        public string Slug { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("ownerId")]
        public long OwnerId { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("logoUrl")]
        public string LogoUrl { get; set; }
        [JsonPropertyName("status")]
        public StoreStatusEnum Status { get; set; }
    }
}
