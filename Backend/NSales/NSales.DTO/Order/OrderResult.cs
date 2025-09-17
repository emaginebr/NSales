using NSales.DTO.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.DTO.Order
{
    public class OrderResult: StatusResult
    {
        public OrderInfo Order { get; set; }
    }
}
