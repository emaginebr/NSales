using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lofn.DTO.ProductType
{
    public class ProductPriceCalculationResult
    {
        [JsonPropertyName("productId")]
        public long ProductId { get; set; }

        [JsonPropertyName("basePriceCents")]
        public long BasePriceCents { get; set; }

        [JsonPropertyName("breakdown")]
        public IList<PriceBreakdownItem> Breakdown { get; set; } = new List<PriceBreakdownItem>();

        [JsonPropertyName("deltaTotalCents")]
        public long DeltaTotalCents { get; set; }

        [JsonPropertyName("totalCents")]
        public long TotalCents { get; set; }
    }
}
