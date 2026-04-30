using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class PriceBreakdownItem
    {
        [JsonPropertyName("optionId")]
        public long OptionId { get; set; }

        [JsonPropertyName("groupLabel")]
        public string GroupLabel { get; set; }

        [JsonPropertyName("optionLabel")]
        public string OptionLabel { get; set; }

        [JsonPropertyName("priceDeltaCents")]
        public long PriceDeltaCents { get; set; }
    }
}
