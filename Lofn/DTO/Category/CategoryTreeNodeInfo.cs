using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.Category
{
    public class CategoryTreeNodeInfo
    {
        [JsonPropertyName("categoryId")]
        public long CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("parentCategoryId")]
        public long? ParentCategoryId { get; set; }

        [JsonPropertyName("isGlobal")]
        public bool IsGlobal { get; set; }

        [JsonPropertyName("productTypeId")]
        public long? ProductTypeId { get; set; }

        [JsonPropertyName("appliedProductTypeId")]
        public long? AppliedProductTypeId { get; set; }

        [JsonPropertyName("children")]
        public IList<CategoryTreeNodeInfo> Children { get; set; } = new List<CategoryTreeNodeInfo>();
    }
}
