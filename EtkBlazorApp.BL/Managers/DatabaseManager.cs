using EtkBlazorApp.DataAccess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class UpdateManager
    {
        private readonly IProductStorage productsStorage;
        private readonly IManufacturerStorage manufacturerStorage;
        private readonly IDatabaseProductCorrelator correlator;

        public UpdateManager(IProductStorage productsStorage, IManufacturerStorage manufacturerStorage, IDatabaseProductCorrelator correlator)
        {
            this.productsStorage = productsStorage;
            this.manufacturerStorage = manufacturerStorage;
            this.correlator = correlator;
        }

        public async Task UpdatePriceAndStock(IEnumerable<PriceLine> priceLines, bool clearStockBeforeUpdate)
        {
            //TODO тут надо будет проверить, т.к. нужно следить что бы в прайс-листах (и шаблонах соответственно) имя производителя точно совпадало с именем из базы данных 
            //Иначе товары не будут загружены и соответственно не обновлены
            //Проблема с Bosch, и возможно другими брендами где pl.manufacturer = null

            var affectedBrands = priceLines.Select(pl => pl.Manufacturer).Distinct().ToList();

            var affectedBrandsIds = (await manufacturerStorage.GetManufacturers())
                .Where(m => affectedBrands.Contains(m.name))
                .Select(m => m.manufacturer_id)
                .ToList();

            var products = await productsStorage.ReadProducts(affectedBrandsIds);

            var data = await correlator.GetCorrelationData(products, priceLines);

            await productsStorage.UpdateProductsPrice(data);
            await productsStorage.UpdateProductsStock(data, clearStockBeforeUpdate);
            
        }
    }
}
