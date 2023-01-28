using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IMarketplaceExportService
    {
        Task<List<MarketplaceBrandExportEntity>> GetAllForMarketplace(string marketplace);
        Task<List<string>> GetAllMarketplaces();
        Task AddOrUpdate(string marketplace, MarketplaceBrandExportEntity data);
        Task Remove(string marketplace, int manufacturer_id);
    }

    public class MarketplaceExportService : IMarketplaceExportService
    {
        private readonly IDatabaseAccess database;

        public MarketplaceExportService(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<MarketplaceBrandExportEntity>> GetAllForMarketplace(string marketplace)
        {
            string sql = @"SELECT mbe.*, m.name as manufacturer_name
                           FROM etk_app_marketplace_brand_export mbe
                           LEFT JOIN oc_manufacturer m ON (m.manufacturer_id = mbe.manufacturer_id)
                           WHERE marketplace = @marketplace
                           ORDER BY m.name";

            var exportInfo = await database.GetList<MarketplaceBrandExportEntity, dynamic>(sql, new { marketplace });
            foreach(var item in exportInfo)
            {
                if(string.IsNullOrWhiteSpace(item.checked_stocks)) { continue; }

                item.checked_stocks_list = item.checked_stocks.Split(',').Select(id => new StockPartnerEntity()
                {
                    stock_partner_id = int.Parse(id)
                }).ToList();
            }

            return exportInfo;
        }

        public async Task Remove(string marketplace, int manufacturer_id)
        {
            string sql = "DELETE FROM etk_app_marketplace_brand_export WHERE marketplace = @marketplace AND manufacturer_id = @manufacturer_id";
            await database.ExecuteQuery<dynamic>(sql, new { marketplace, manufacturer_id });
        }
        
        public async Task AddOrUpdate(string marketplace, MarketplaceBrandExportEntity data)
        {
            var sql = @"INSERT INTO etk_app_marketplace_brand_export
                           (marketplace, manufacturer_id, discount, checked_stocks)
                               VALUES
                               (@marketplace, @manufacturer_id, @discount, @checked_stocks)
                               ON DUPLICATE KEY UPDATE
                               discount = @discount,
                               checked_stocks = @checked_stocks";

            string separetedCheckedStocks = string.Join(",", data.checked_stocks_list?.Select(s => s.stock_partner_id));

            await database.ExecuteQuery<dynamic>(sql, new 
            { 
                marketplace,
                manufacturer_id = data.manufacturer_id,
                discount = data.discount,
                checked_stocks = separetedCheckedStocks
            });
        }

        public async Task<List<string>> GetAllMarketplaces()
        {
            string sql = "SELECT DISTINCT marketplace FROM etk_app_marketplace_brand_export";

            var data = await database.GetList<string>(sql);

            return data;
        }
    }
}
