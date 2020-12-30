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
        private readonly ISettingStorage settingsStorage;
        private List<ShopAccountEntity> monobrandAccountConnections;

        public DatabaseManager(ISettingStorage settingsStorage)
        {
            this.settingsStorage = settingsStorage;
        }

        public async Task<List<ShopAccountEntity>> GetMonobrandAccountConnections()
        {
            await RefreshAccountList();
            return monobrandAccountConnections;
        }

        public async Task RefreshAccountList(bool force = false)
        {
            if (monobrandAccountConnections == null || force)
            {
                monobrandAccountConnections = await settingsStorage.GetShopAccounts();
            }
        }

        public async Task UpdatePriceAndStock(int website_id, List<ProductUpdateData> data, bool clearStockBeforeUpdate)
        {
            if (data.Any())
            {
                var monobrand = GetConnectionById(website_id);

                await monobrand.UpdateProducts(data, clearStockBeforeUpdate);
            }
        }

        public async Task<List<ProductEntity>> ReadProducts(int website_id)
        {
            var monobrand = GetConnectionById(website_id);

            var products = await monobrand.ReadProducts();

            return products;
        }

        private MonobrandConnection GetConnectionById(int website_id)
        {
            var connectionInfo = monobrandAccountConnections.Single(conn => conn.website_id == website_id);

            var monobrandConnection = new MonobrandConnection(connectionInfo);

            return monobrandConnection;
        }
    }
}
