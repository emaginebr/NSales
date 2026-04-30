using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class ProductPriceCalculationRequest
    {
        [JsonPropertyName("optionIds")]
        public IList<long> OptionIds { get; set; } = new List<long>();
    }
}
