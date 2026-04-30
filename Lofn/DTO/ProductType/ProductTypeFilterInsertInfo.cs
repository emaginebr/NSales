using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class ProductTypeFilterInsertInfo
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        [JsonPropertyName("isRequired")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("allowedValues")]
        public IList<string> AllowedValues { get; set; }
    }
}
