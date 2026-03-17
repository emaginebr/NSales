using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lofn.DTO.Product
{
    public class ProductSearchInternalParam
    {
        [JsonPropertyName("networkId")]
        public long? NetworkId { get; set; }
        [JsonPropertyName("userId")]
        public long? UserId { get; set; }
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }
        [JsonPropertyName("onlyActive")]
        public bool OnlyActive { get; set; }
        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }
    }
}
