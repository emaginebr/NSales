using System.Text.Json.Serialization;

namespace Lofn.DTO.Category
{
    public class CategoryInfo
    {
        [JsonPropertyName("categoryId")]
        public long CategoryId { get; set; }
        [JsonPropertyName("slug")]
        public string Slug { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("storeId")]
        public long? StoreId { get; set; }
        [JsonPropertyName("isGlobal")]
        public bool IsGlobal { get; set; }
        [JsonPropertyName("parentCategoryId")]
        public long? ParentCategoryId { get; set; }
        [JsonPropertyName("productCount")]
        public int ProductCount { get; set; }
    }
}
