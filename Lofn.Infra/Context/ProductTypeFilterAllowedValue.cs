namespace Lofn.Infra.Context;

public partial class ProductTypeFilterAllowedValue
{
    public long AllowedValueId { get; set; }

    public long FilterId { get; set; }

    public string Value { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ProductTypeFilter Filter { get; set; }
}
