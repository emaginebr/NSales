using System;

namespace Lofn.Infra.Context;

public partial class ProductFilterValue
{
    public long ProductFilterValueId { get; set; }

    public long ProductId { get; set; }

    public long FilterId { get; set; }

    // Stringified value; interpretation governed by Filter.DataType.
    public string Value { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; }

    public virtual ProductTypeFilter Filter { get; set; }
}
