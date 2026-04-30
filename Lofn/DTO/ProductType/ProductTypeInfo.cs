using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class ProductTypeInfo
    {
        [JsonPropertyName("productTypeId")]
        public long ProductTypeId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("filters")]
        public IList<ProductTypeFilterInfo> Filters { get; set; } = new List<ProductTypeFilterInfo>();

        [JsonPropertyName("customizationGroups")]
        public IList<CustomizationGroupInfo> CustomizationGroups { get; set; } = new List<CustomizationGroupInfo>();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
