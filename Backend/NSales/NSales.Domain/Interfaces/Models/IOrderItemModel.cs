using NSales.Domain.Interfaces.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Interfaces.Models
{
    public interface IOrderItemModel
    {
        long ItemId {  get; set; }
        long OrderId { get; set; }
        long ProductId { get; set; }
        int Quantity { get; set; }
        IProductModel GetProduct(IProductDomainFactory factory);
        IList<IOrderItemModel> ListItems(long orderId, IOrderItemDomainFactory factory);
        IOrderItemModel Insert(IOrderItemDomainFactory factory);
    }
}
