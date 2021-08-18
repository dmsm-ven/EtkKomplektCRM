using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IManufacturerStorage
    {
        Task<List<ManufacturerEntity>> GetManufacturers();
        Task<List<StockPartnerManufacturerInfoEntity>> GetManufacturerStockPartners(int manufacturer_id);

        Task CreateOrUpdateStock(StockPartnerEntity stock);
        Task<List<StockPartnerEntity>> GetStocks();
        Task<List<StockCityEntity>> GetStockCities();
        Task<List<StockPartnerLinkedManufacturerInfoEntity>> GetStockManufacturers(int stock_partner_id);
    }

    public class ManufacturerStorage : IManufacturerStorage
    {
        private readonly IDatabaseAccess database;

        public ManufacturerStorage(IDatabaseAccess database)
        {
            this.database = database;
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
   
        public async Task<List<StockPartnerEntity>> GetStocks()
        {
            string sql = @"SELECT sp.*, sc.name as city
                          FROM oc_stock_partner sp
                          LEFT JOIN oc_stock_city sc ON sp.city_id = sc.city_id
                          ORDER BY sp.shipment_period";

            var stocks = await database.GetList<StockPartnerEntity>(sql);

            return stocks;
        }

        public async Task CreateOrUpdateStock(StockPartnerEntity stock)
        {
            if(stock.city_id == -1)
            {
                await database.ExecuteQuery("INSERT INTO oc_stock_city (name) VALUES (@city)", stock);
                stock.city_id = await database.GetScalar<int>("SELECT max(city_id) FROM oc_stock_city");
            }
            string sql = @"INSERT INTO oc_stock_partner (stock_partner_id, shipment_period, city_id, name, description, phone_number, address, email, website, show_name_for_all)
                         VALUES (@stock_partner_id, @shipment_period, @city_id, @name, @description, @phone_number, @address, @email, @website, @show_name_for_all)
                         ON DUPLICATE KEY 
                            UPDATE shipment_period = @shipment_period,
                            city_id = @city_id,
                            name = @name,
                            description = @description,
                            phone_number = @phone_number,
                            address = @address,
                            email = @email,
                            website = @website,
                            show_name_for_all = @show_name_for_all";

            await database.ExecuteQuery(sql, stock);

            if (stock.stock_partner_id == 0)
            {
                stock.stock_partner_id = await database.GetScalar<int>("SELECT max(stock_partner_id) FROM oc_stock_partner");
            }
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

        public async Task<List<StockCityEntity>> GetStockCities()
        {
            string sql = @"SELECT * FROM oc_stock_city ORDER BY city_id";

            var cities = await database.GetList<StockCityEntity>(sql);

            return cities;
        }
    }
}
