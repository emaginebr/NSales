using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class CustomizationOptionInsertInfo
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("priceDeltaCents")]
        public long PriceDeltaCents { get; set; }

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }
    }
}
