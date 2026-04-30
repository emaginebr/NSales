using System;
using System.Collections.Generic;

namespace Lofn.Infra.Context;

public partial class Product
{
    public long ProductId { get; set; }

    public long UserId { get; set; }

    public string Slug { get; set; }

    public string Name { get; set; }

    public double Price { get; set; }

    public double Discount { get; set; }

    public int Frequency { get; set; }

    public int Limit { get; set; }

    public int Status { get; set; }

    public int ProductType { get; set; }

    public string Description { get; set; }

    public long? StoreId { get; set; }

    public long? CategoryId { get; set; }

    public bool Featured { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Store Store { get; set; }

    public virtual Category Category { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductFilterValue> FilterValues { get; set; } = new List<ProductFilterValue>();
}
