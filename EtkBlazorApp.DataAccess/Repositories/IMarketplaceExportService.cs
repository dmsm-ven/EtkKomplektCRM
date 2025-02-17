using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Entity.Manufacturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Repositories
{
    public interface IMarketplaceExportService
    {
        Task<List<MarketplaceBrandExportEntity>> GetAllForMarketplace(string marketplace);
        Task<List<string>> GetAllMarketplaces();
        Task AddOrUpdate(string marketplace, MarketplaceBrandExportEntity data);
        Task Remove(string marketplace, int manufacturer_id);

        Task<List<MarketplaceStepDiscountEntity>> GetAllStepDiscounts();
        Task AddStepDiscount(string marketplace, int priceInRub, decimal ratio);
        Task RemoveStepDiscount(string marketplace, int priceInRub);

        Task<List<ManufacturerEntity>> GetOzonInOrderStockManufacturers();
        Task AddOzonInOrderStockManufacturer(int manufacturer_id);
        Task RemoveOzonInOrderStockManufacturer(int manufacturer_id);
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
            foreach (var item in exportInfo)
            {
                if (string.IsNullOrWhiteSpace(item.checked_stocks)) { continue; }

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
                data.manufacturer_id,
                data.discount,
                checked_stocks = separetedCheckedStocks
            });
        }

        public async Task<List<string>> GetAllMarketplaces()
        {
            string sql = "SELECT DISTINCT marketplace FROM etk_app_marketplace_brand_export";

            var data = await database.GetList<string>(sql);

            return data;
        }

        public async Task<List<MarketplaceStepDiscountEntity>> GetAllStepDiscounts()
        {
            string sql = "SELECT `oc_setting`.`value` FROM oc_setting WHERE `oc_setting`.`key` = 'marketplace_discount_steps_all'";
            var list = new List<MarketplaceStepDiscountEntity>();

            try
            {
                var json = await database.GetScalar<string>(sql);

                var dic = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);

                list.AddRange(dic.Select(kvp => new MarketplaceStepDiscountEntity()
                {
                    marketplace = "all",
                    price_border_in_rub = int.Parse(kvp.Key),
                    ratio = kvp.Value
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }

        public async Task AddStepDiscount(string marketplace, int priceInRub, decimal ratio)
        {
            string setting_key = $"marketplace_discount_steps_{marketplace}";
            string sql = $@"UPDATE oc_setting
                            SET oc_setting.value = JSON_SET(oc_setting.value, '$.{priceInRub}', {ratio.ToString().Replace(",", ".")})
                            WHERE oc_setting.key = @setting_key AND serialized = 1";

            try
            {
                await database.ExecuteQuery(sql, new { setting_key, ratio });
            }
            catch
            {

            }
        }

        public async Task RemoveStepDiscount(string marketplace, int priceInRub)
        {
            string setting_key = $"marketplace_discount_steps_{marketplace}";
            string sql = $@"UPDATE oc_setting
                            SET oc_setting.value = JSON_REMOVE(oc_setting.value, '$.{priceInRub}')
                            WHERE oc_setting.key = @setting_key AND serialized = 1";

            try
            {
                await database.ExecuteQuery(sql, new { setting_key });
            }
            catch
            {

            }

        }

        public async Task<List<ManufacturerEntity>> GetOzonInOrderStockManufacturers()
        {
            string sql = @"SELECT m.* 
                            FROM etk_app_ozon_seller_in_order_manufacturer om
                            JOIN oc_manufacturer m ON (om.manufacturer_id = m.manufacturer_id)";

            var list = await database.GetList<ManufacturerEntity>(sql);

            return list;
        }

        public async Task AddOzonInOrderStockManufacturer(int manufacturer_id)
        {
            await database.ExecuteQuery("INSERT IGNORE INTO etk_app_ozon_seller_in_order_manufacturer (manufacturer_id) VALUES (@manufacturer_id)", new { manufacturer_id });

        }

        public async Task RemoveOzonInOrderStockManufacturer(int manufacturer_id)
        {
            await database.ExecuteQuery("DELETE FROM etk_app_ozon_seller_in_order_manufacturer WHERE manufacturer_id = @manufacturer_id", new { manufacturer_id });
        }
    }
}
