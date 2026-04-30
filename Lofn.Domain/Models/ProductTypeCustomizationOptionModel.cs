namespace Lofn.Domain.Models
{
    public class ProductTypeCustomizationOptionModel
    {
        public long OptionId { get; set; }

        public long GroupId { get; set; }

        public string Label { get; set; }

        public long PriceDeltaCents { get; set; }

        public bool IsDefault { get; set; }

        public int DisplayOrder { get; set; }
    }
}
