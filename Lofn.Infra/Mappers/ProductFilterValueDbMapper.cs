using Lofn.Domain.Models;
using Lofn.Infra.Context;

namespace Lofn.Infra.Mappers
{
    public static class ProductFilterValueDbMapper
    {
        public static ProductFilterValueModel ToModel(ProductFilterValue row, ProductTypeFilter filter)
        {
            return new ProductFilterValueModel
            {
                ProductFilterValueId = row.ProductFilterValueId,
                ProductId = row.ProductId,
                FilterId = row.FilterId,
                FilterLabel = filter?.Label,
                DataType = filter?.DataType,
                Value = row.Value
            };
        }
    }
}
