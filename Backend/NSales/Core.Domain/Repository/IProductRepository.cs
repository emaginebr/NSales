using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Repository
{
    public interface IProductRepository<TModel, TFactory>
    {
        IEnumerable<TModel> Search(long? networkId, long? userId, string keyword, bool active, int pageNum, out int pageCount, TFactory factory);
        IEnumerable<TModel> ListByNetwork(long networkId, TFactory factory);
        TModel GetById(long id, TFactory factory);
        TModel GetBySlug(string slug, TFactory factory);
        TModel GetByStripeProductId(string stripeProductId, TFactory factory);
        TModel GetByStripePriceId(string stripePriceId, TFactory factory);
        TModel Insert(TModel model, TFactory factory);
        TModel Update(TModel model, TFactory factory);
        bool ExistSlug(long productId, string slug);
    }
}
