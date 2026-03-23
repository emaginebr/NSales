using Lofn.Domain.Interfaces;
using Lofn.DTO.ShopCart;
using Lofn.Infra.Interfaces.AppService;
using System.Threading.Tasks;

namespace Lofn.Domain.Services
{
    public class ShopCartService : IShopCartService
    {
        private readonly IRabbitMQAppService _rabbitMQAppService;

        public ShopCartService(IRabbitMQAppService rabbitMQAppService)
        {
            _rabbitMQAppService = rabbitMQAppService;
        }

        public async Task<ShopCartInfo> InsertAsync(ShopCartInfo shopCart)
        {
            await _rabbitMQAppService.PublishAsync(shopCart);
            return shopCart;
        }
    }
}
