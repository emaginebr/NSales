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
        [JsonPropertyName("address")]
        public ShopCartAddressInfo Address { get; set; }
        [JsonPropertyName("items")]
        public IList<ShopCartItemInfo> Items { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class ShopCartAddressInfo
    {
        [JsonPropertyName("zipCode")]
        public string ZipCode { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("complement")]
        public string Complement { get; set; }
        [JsonPropertyName("neighborhood")]
        public string Neighborhood { get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; }
        [JsonPropertyName("state")]
        public string State { get; set; }
    }

    public class ShopCartItemInfo
    {
        [JsonPropertyName("product")]
        public ProductInfo Product { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
