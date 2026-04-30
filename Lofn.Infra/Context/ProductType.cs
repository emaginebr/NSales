using System;
using System.Collections.Generic;

namespace Lofn.Infra.Context;

// Feature 003-product-type-filters: tenant-scoped classifier defined by admin.
// Distinct from the legacy Product.ProductType int field (Physical/InfoProduct enum).
public partial class ProductType
{
    public long ProductTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ProductTypeFilter> Filters { get; set; } = new List<ProductTypeFilter>();

    public virtual ICollection<ProductTypeCustomizationGroup> CustomizationGroups { get; set; } = new List<ProductTypeCustomizationGroup>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
