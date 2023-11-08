using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface IDatabaseProductCorrelator
    {
        Task<List<ProductUpdateData>> GetCorrelationData(IEnumerable<ProductEntity> products, IEnumerable<PriceLine> priceLines);
    }
}
