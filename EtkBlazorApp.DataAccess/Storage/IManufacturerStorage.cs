using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IManufacturerStorage
    {
        Task SaveManufacturer(ManufacturerEntity manufacturer);
        Task<List<ManufacturerEntity>> GetManufacturers();
        Task<List<StockPartnerManufacturerInfoEntity>> GetManufacturerStockPartners(int manufacturer_id);

        Task SaveStockPartner(StockPartnerEntity stock);
        Task<List<StockPartnerEntity>> GetStockPartners();
        Task CreateStock(StockPartnerEntity stock);
        Task<List<StockPartnerLinkedManufacturerInfoEntity>> GetStockManufacturers(int stock_partner_id);
    }

    public class ManufacturerStorage : IManufacturerStorage
    {
        private readonly IDatabaseAccess database;

        public ManufacturerStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task SaveManufacturer(ManufacturerEntity manufacturer)
        {
            string sql = @"UPDATE oc_manufacturer
                         SET shipment_period = @shipment_period
                         WHERE manufacturer_id = @manufacturer_id";
            await database.ExecuteQuery(sql, manufacturer);
        }

        public async Task<List<ManufacturerEntity>> GetManufacturers()
        {
            string sql = @"SELECT m.*, url.keyword
                         FROM oc_manufacturer m
                         LEFT JOIN oc_url_alias url ON CONCAT('manufacturer_id=', m.manufacturer_id) = url.query
                         ORDER BY name";
            var manufacturers = await database.GetList<ManufacturerEntity, dynamic>(sql, new { });
            return manufacturers;
        }            
    
        public async Task SaveStockPartner(StockPartnerEntity stock)
        {
            string sql = @"UPDATE oc_stock_partner
                         SET shipment_period = @shipment_period
                         WHERE stock_partner_id = @stock_partner_id";
            await database.ExecuteQuery(sql, stock);
        }

        public async Task<List<StockPartnerEntity>> GetStockPartners()
        {
            string sql = @"SELECT * FROM oc_stock_partner ORDER BY name";

            var stocks = await database.GetList<StockPartnerEntity>(sql);

            return stocks;
        }

        public async Task CreateStock(StockPartnerEntity stock)
        {
            string sql = @"INSERT INTO oc_stock_partner (name, description, shipment_period) VALUES 
                                                        (@name, @description, @shipment_period)";
           
            await database.ExecuteQuery(sql, stock);

            stock.stock_partner_id = await database.GetScalar<int>("SELECT max(stock_partner_id) FROM oc_stock_partner");
        }

        public async Task<List<StockPartnerManufacturerInfoEntity>> GetManufacturerStockPartners(int manufacturer_id)
        {
            string sql = @"SELECT sp.stock_partner_id, sp.name, count(p.product_id) as total_products, sum(ptc.quantity) as total_quantity
                           FROM oc_product_to_stock ptc
                           JOIN oc_stock_partner sp ON (ptc.stock_partner_id = sp.stock_partner_id)
                           JOIN oc_product p ON (ptc.product_id = p.product_id)
                           WHERE p.manufacturer_id = @manufacturer_id
                           GROUP BY stock_partner_id, name
                           ORDER BY count(p.product_id) DESC";

            var stocks = await database.GetList<StockPartnerManufacturerInfoEntity, dynamic>(sql, new { manufacturer_id });

            return stocks;
        }

        public async Task<List<StockPartnerLinkedManufacturerInfoEntity>> GetStockManufacturers(int stock_partner_id)
        {
            string sql = @"SELECT m.manufacturer_id, m.name, count(p.product_id) as total_products, sum(ptc.quantity) as total_quantity
                           FROM oc_product_to_stock ptc
                           JOIN oc_stock_partner sp ON (ptc.stock_partner_id = sp.stock_partner_id)
                           JOIN oc_product p ON (ptc.product_id = p.product_id)
                           JOIN oc_manufacturer m ON (m.manufacturer_id = p.manufacturer_id)
                           WHERE ptc.stock_partner_id = @stock_partner_id
                           GROUP BY m.name
                           ORDER BY count(p.product_id) DESC";

            var linkedBrands = await database.GetList<StockPartnerLinkedManufacturerInfoEntity, dynamic>(sql, new { stock_partner_id });

            return linkedBrands;
        }
    }
}
