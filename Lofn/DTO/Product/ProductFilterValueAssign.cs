using System.Text.Json.Serialization;

namespace Lofn.DTO.Product
{
    public class ProductFilterValueAssign
    {
        [JsonPropertyName("filterId")]
        public long FilterId { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
