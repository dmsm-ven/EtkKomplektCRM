using EtkBlazorApp.BL.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
{
    public class DatabaseManager
    {
        private readonly IProductStorage productsAccess;

        public DatabaseManager(IProductStorage productsAccess)
        {
            this.productsAccess = productsAccess;
        }

        public async Task UpdatePriceAndStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate)
        {
            await productsAccess.UpdateStock(data, clearStockBeforeUpdate);
            await productsAccess.UpdatePrice(data);
        }

        public async Task<List<ProductEntity>> ReadProducts()
        {
            var products = await productsAccess.ReadProducts();

            return products;
        }
    }
}
