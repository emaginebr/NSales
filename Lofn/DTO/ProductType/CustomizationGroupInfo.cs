using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class CustomizationGroupInfo
    {
        [JsonPropertyName("groupId")]
        public long GroupId { get; set; }

        [JsonPropertyName("productTypeId")]
        public long ProductTypeId { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("selectionMode")]
        public string SelectionMode { get; set; }

        [JsonPropertyName("isRequired")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("options")]
        public IList<CustomizationOptionInfo> Options { get; set; } = new List<CustomizationOptionInfo>();
    }
}
