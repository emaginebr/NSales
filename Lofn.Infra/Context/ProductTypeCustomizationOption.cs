using System;

namespace Lofn.Infra.Context;

public partial class ProductTypeCustomizationOption
{
    public long OptionId { get; set; }

    public long GroupId { get; set; }

    public string Label { get; set; }

    // signed cents — positive (upcharge), negative (discount), or zero
    public long PriceDeltaCents { get; set; }

    public bool IsDefault { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ProductTypeCustomizationGroup Group { get; set; }
}
