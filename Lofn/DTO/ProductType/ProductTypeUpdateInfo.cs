using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class ProductTypeUpdateInfo
    {
        [JsonPropertyName("productTypeId")]
        public long ProductTypeId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
