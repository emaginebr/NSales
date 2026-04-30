using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class ProductTypeInsertInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
