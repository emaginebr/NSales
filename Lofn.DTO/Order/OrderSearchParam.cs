using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lofn.DTO.Order
{
    public class OrderSearchParam
    {
        [JsonPropertyName("networkId")]
        public long NetworkId { get; set; }
        [JsonPropertyName("userId")]
        public long? UserId { get; set; }
        [JsonPropertyName("sellerId")]
        public long? SellerId { get; set; }
        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }
    }
}
