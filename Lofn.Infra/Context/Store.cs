using System;
using System.Collections.Generic;

namespace Lofn.Infra.Context;

public partial class Store
{
    public long StoreId { get; set; }

    public string Slug { get; set; }

    public string Name { get; set; }

    public long OwnerId { get; set; }

    public string Logo { get; set; }

    public int Status { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<StoreUser> StoreUsers { get; set; } = new List<StoreUser>();
}
