using System;
using System.Collections.Generic;

namespace Lofn.Infra.Context;

public partial class OrderItem
{
    public long ItemId { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Order Order { get; set; }

    public virtual Product Product { get; set; }
}
