namespace Lofn.Domain.Models
{
    public class ProductFilterValueModel
    {
        public long ProductFilterValueId { get; set; }

        public long ProductId { get; set; }

        public long FilterId { get; set; }

        public string FilterLabel { get; set; }

        // Mirrors ProductTypeFilterModel.DataType so consumers can interpret Value without re-querying.
        public string DataType { get; set; }

        public string Value { get; set; }
    }
}
