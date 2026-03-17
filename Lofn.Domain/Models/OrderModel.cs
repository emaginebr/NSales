using Lofn.DTO.Order;
using System;

namespace Lofn.Domain.Models
{
    public class OrderModel
    {
        public long OrderId { get; set; }
        public long? NetworkId { get; set; }
        public long UserId { get; set; }
        public long? SellerId { get; set; }
        public OrderStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string StripeId { get; set; }
    }
}
