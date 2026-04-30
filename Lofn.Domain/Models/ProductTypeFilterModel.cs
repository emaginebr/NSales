using System.Collections.Generic;

namespace Lofn.Domain.Models
{
    public class ProductTypeFilterModel
    {
        public long FilterId { get; set; }

        public long ProductTypeId { get; set; }

        public string Label { get; set; }

        // text | integer | decimal | boolean | enum
        public string DataType { get; set; }

        public bool IsRequired { get; set; }

        public int DisplayOrder { get; set; }

        public IList<string> AllowedValues { get; set; } = new List<string>();
    }
}
