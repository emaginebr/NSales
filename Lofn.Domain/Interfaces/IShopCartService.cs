using Lofn.DTO.ShopCart;
using System.Threading.Tasks;

namespace Lofn.Domain.Interfaces
{
    public interface IShopCartService
    {
        Task<ShopCartInfo> InsertAsync(ShopCartInfo shopCart);
    }
}
