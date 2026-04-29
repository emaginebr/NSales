using System.Text.Json.Serialization;

namespace Lofn.DTO.Category
{
    public class CategoryGlobalInsertInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("parentCategoryId")]
        public long? ParentCategoryId { get; set; }
    }
}
