using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class ProductFilterValueInfo
    {
        [JsonPropertyName("filterId")]
        public long FilterId { get; set; }

        [JsonPropertyName("filterLabel")]
        public string FilterLabel { get; set; }

        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
