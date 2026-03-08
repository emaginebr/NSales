using System;
using System.Collections.Generic;

namespace DB.Infra.Context;

public partial class Order
{
    public long OrderId { get; set; }

    public long UserId { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long? SellerId { get; set; }

    public long? NetworkId { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
