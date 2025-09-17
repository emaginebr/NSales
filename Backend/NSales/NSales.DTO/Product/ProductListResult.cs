using NSales.DTO.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.DTO.Product
{
    public class ProductListResult: StatusResult
    {
        public IList<ProductInfo> Products { get; set; }
    }
}
