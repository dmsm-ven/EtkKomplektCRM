using EtkBlazorApp.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
{
    public class UpdateManager
    {
        private readonly IProductStorage productsStorage;
        private readonly IDatabaseProductCorrelator correlator;

        public UpdateManager(IProductStorage productsStorage, IDatabaseProductCorrelator correlator)
        {
            this.productsStorage = productsStorage;
            this.correlator = correlator;
        }

        public async Task UpdatePriceAndStock(IEnumerable<PriceLine> priceLines, bool clearStockBeforeUpdate)
        {            
            var products = await productsStorage.ReadProducts();

            var data = await correlator.GetCorrelationData(products, priceLines);

            await productsStorage.UpdateStock(data, clearStockBeforeUpdate);
            await productsStorage.UpdatePrice(data);
        }
    }
}
