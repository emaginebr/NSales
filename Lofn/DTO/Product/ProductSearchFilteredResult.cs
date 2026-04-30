using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.Product
{
    public class ProductSearchFilteredResult
    {
        [JsonPropertyName("products")]
        public IList<ProductInfo> Products { get; set; } = new List<ProductInfo>();

        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }

        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }

        [JsonPropertyName("appliedProductTypeId")]
        public long? AppliedProductTypeId { get; set; }

        [JsonPropertyName("appliedFilters")]
        public IList<AppliedFilterInfo> AppliedFilters { get; set; } = new List<AppliedFilterInfo>();

        [JsonPropertyName("ignoredFilterIds")]
        public IList<long> IgnoredFilterIds { get; set; } = new List<long>();
    }
}
