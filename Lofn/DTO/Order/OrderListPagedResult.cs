using Lofn.DTO.Domain;
using Lofn.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lofn.DTO.Order
{
    public class OrderListPagedResult: StatusResult
    {
        [JsonPropertyName("orders")]
        public IList<OrderInfo> Orders { get; set; }
        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }
        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }
    }
}
