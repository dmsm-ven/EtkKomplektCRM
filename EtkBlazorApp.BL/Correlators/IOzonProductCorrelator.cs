using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface IOzonProductCorrelator
    {
        Dictionary<OzonProductModel, ProductEntity> GetCorrelationData(IEnumerable<OzonProductModel> offers, IEnumerable<ProductEntity> products);
    }

    public class SimpleOzonProductCorrelator : IOzonProductCorrelator
    {
        public Dictionary<OzonProductModel, ProductEntity> GetCorrelationData(IEnumerable<OzonProductModel> offers,
            IEnumerable<ProductEntity> products)
        {
            var dic = offers.Select(offer => new
            {
                Offer = offer,
                OpencartProduct = products.FirstOrDefault(pl => pl.sku.Equals(offer.offer_id) || pl.model.Equals(offer.offer_id))
            })
                .ToDictionary(i => i.Offer, j => j.OpencartProduct);
            return dic;
        }
    }
}
