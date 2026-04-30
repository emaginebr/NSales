using System.Text.Json.Serialization;

namespace Lofn.DTO.Product
{
    public class AppliedFilterInfo
    {
        [JsonPropertyName("filterId")]
        public long FilterId { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
