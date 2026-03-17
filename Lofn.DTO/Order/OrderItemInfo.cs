using Lofn.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lofn.DTO.Order
{
    public class OrderItemInfo
    {
        [JsonPropertyName("itemId")]
        public long ItemId { get; set; }
        [JsonPropertyName("orderId")]
        public long OrderId { get; set; }
        [JsonPropertyName("productId")]
        public long ProductId { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("product")]
        public ProductInfo Product { get; set; }
    }
}
