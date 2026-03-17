using Lofn.DTO.Product;

namespace Lofn.Domain.Models
{
    public class ProductModel
    {
        public long ProductId { get; set; }
        public long? NetworkId { get; set; }
        public long UserId { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Frequency { get; set; }
        public int Limit { get; set; }
        public ProductStatusEnum Status { get; set; }
        public string StripeProductId { get; set; }
        public string StripePriceId { get; set; }
    }
}
