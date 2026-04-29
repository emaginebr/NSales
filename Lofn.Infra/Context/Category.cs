using System;
using System.Collections.Generic;

namespace Lofn.Infra.Context;

public partial class Category
{
    public long CategoryId { get; set; }

    public string Slug { get; set; }

    public string Name { get; set; }

    public long? StoreId { get; set; }

    public long? ParentId { get; set; }

    public virtual Store Store { get; set; }

    public virtual Category Parent { get; set; }

    public virtual ICollection<Category> Children { get; set; } = new List<Category>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
