using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.Product
{
    public class ProductInsertInfo
    {
        [JsonPropertyName("categoryId")]
        public long? CategoryId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("price")]
        public double Price { get; set; }
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        [JsonPropertyName("status")]
        public ProductStatusEnum Status { get; set; }
        [JsonPropertyName("productType")]
        public ProductTypeEnum ProductType { get; set; }
        [JsonPropertyName("featured")]
        public bool Featured { get; set; }

        [JsonPropertyName("filterValues")]
        public IList<ProductFilterValueAssign> FilterValues { get; set; }
    }
}
