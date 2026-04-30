using System;
using System.Collections.Generic;

namespace Lofn.Domain.Models
{
    public class ProductTypeModel
    {
        public long ProductTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public IList<ProductTypeFilterModel> Filters { get; set; } = new List<ProductTypeFilterModel>();

        public IList<ProductTypeCustomizationGroupModel> CustomizationGroups { get; set; } = new List<ProductTypeCustomizationGroupModel>();
    }
}
