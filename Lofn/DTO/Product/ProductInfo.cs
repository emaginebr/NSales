using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Lofn.DTO.ProductType;

namespace Lofn.DTO.Product
{
    public class ProductInfo
    {
        [JsonPropertyName("productId")]
        public long ProductId { get; set; }
        [JsonPropertyName("storeId")]
        public long? StoreId { get; set; }
        [JsonPropertyName("categoryId")]
        public long? CategoryId { get; set; }
        [JsonPropertyName("slug")]
        public string Slug { get; set; }
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
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("images")]
        public IList<ProductImageInfo> Images { get; set; }

        [JsonPropertyName("filterValues")]
        public IList<ProductFilterValueInfo> FilterValues { get; set; }

        [JsonPropertyName("appliedProductTypeId")]
        public long? AppliedProductTypeId { get; set; }
    }
}
