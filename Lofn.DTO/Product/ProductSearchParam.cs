using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lofn.DTO.Product
{
    public class ProductSearchParam: ProductSearchInternalParam
    {
        [JsonPropertyName("userSlug")]
        public string UserSlug { get; set; }
        [JsonPropertyName("networkSlug")]
        public string NetworkSlug { get; set; }
    }
}
