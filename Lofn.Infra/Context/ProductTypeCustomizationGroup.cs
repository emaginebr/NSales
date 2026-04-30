using System;
using System.Collections.Generic;

namespace Lofn.Infra.Context;

public partial class ProductTypeCustomizationGroup
{
    public long GroupId { get; set; }

    public long ProductTypeId { get; set; }

    public string Label { get; set; }

    // single | multi
    public string SelectionMode { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ProductType ProductType { get; set; }

    public virtual ICollection<ProductTypeCustomizationOption> Options { get; set; } = new List<ProductTypeCustomizationOption>();
}
