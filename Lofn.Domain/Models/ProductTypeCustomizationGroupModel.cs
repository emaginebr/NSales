using System.Collections.Generic;

namespace Lofn.Domain.Models
{
    public class ProductTypeCustomizationGroupModel
    {
        public long GroupId { get; set; }

        public long ProductTypeId { get; set; }

        public string Label { get; set; }

        // single | multi
        public string SelectionMode { get; set; }

        public bool IsRequired { get; set; }

        public int DisplayOrder { get; set; }

        public IList<ProductTypeCustomizationOptionModel> Options { get; set; } = new List<ProductTypeCustomizationOptionModel>();
    }
}
