using System.Text.Json.Serialization;

namespace Lofn.DTO.Category
{
    public class CategoryGlobalUpdateInfo
    {
        [JsonPropertyName("categoryId")]
        public long CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("parentCategoryId")]
        public long? ParentCategoryId { get; set; }

        [JsonPropertyName("productTypeId")]
        public long? ProductTypeId { get; set; }
    }
}
