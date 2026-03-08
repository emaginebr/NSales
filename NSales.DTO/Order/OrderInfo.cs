using NAuth.DTO.User;
using NSales.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NSales.DTO.Order
{
    public class OrderInfo
    {
        [JsonPropertyName("orderId")]
        public long OrderId { get; set; }
        [JsonPropertyName("networkId")]
        public long? NetworkId { get; set; }
        [JsonPropertyName("userId")]
        public long UserId { get; set; }
        [JsonPropertyName("sellerId")]
        public long? SellerId { get; set; }
        [JsonPropertyName("status")]
        public OrderStatusEnum Status { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("user")]
        public UserInfo User { get; set; }
        [JsonPropertyName("seller")]
        public UserInfo Seller { get; set; }
        [JsonPropertyName("items")]
        public IList<OrderItemInfo> Items { get; set; }

    }
}
