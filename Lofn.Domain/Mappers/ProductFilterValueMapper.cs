using Lofn.Domain.Models;
using Lofn.DTO.ProductType;

namespace Lofn.Domain.Mappers
{
    public static class ProductFilterValueMapper
    {
        public static ProductFilterValueInfo ToInfo(ProductFilterValueModel md)
        {
            if (md == null) return null;
            return new ProductFilterValueInfo
            {
                FilterId = md.FilterId,
                FilterLabel = md.FilterLabel,
                DataType = md.DataType,
                Value = md.Value
            };
        }
    }
}
