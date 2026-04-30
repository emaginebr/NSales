using System;
using System.Collections.Generic;

namespace Lofn.Infra.Context;

public partial class ProductTypeFilter
{
    public long FilterId { get; set; }

    public long ProductTypeId { get; set; }

    public string Label { get; set; }

    // Discriminator: text | integer | decimal | boolean | enum
    public string DataType { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ProductType ProductType { get; set; }

    public virtual ICollection<ProductTypeFilterAllowedValue> AllowedValues { get; set; } = new List<ProductTypeFilterAllowedValue>();
}
