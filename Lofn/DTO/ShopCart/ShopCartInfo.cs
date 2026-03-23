using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Lofn.DTO.Product;
using NAuth.DTO.User;

namespace Lofn.DTO.ShopCart
{
    public class ShopCartInfo
    {
        [JsonPropertyName("user")]
        public UserInfo User { get; set; }
        [JsonPropertyName("items")]
        public IList<ShopCartItemInfo> Items { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class ShopCartItemInfo
    {
        [JsonPropertyName("product")]
        public ProductInfo Product { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
