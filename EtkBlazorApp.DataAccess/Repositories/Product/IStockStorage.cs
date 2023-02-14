using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IStockStorage
    {
        Task CreateOrUpdateStock(StockPartnerEntity stock);
        Task<List<StockPartnerEntity>> GetStocks();
        Task<StockPartnerEntity> GetStockById(int stock_partner_id);

        Task<List<StockCityEntity>> GetStockCities();
        Task<List<StockPartnerLinkedManufacturerInfoEntity>> GetStockManufacturers(int stock_partner_id);
        Task<List<StockPartnerManufacturerInfoEntity>> GetManufacturerStockPartners(int manufacturer_id);
        Task<List<ManufacturerAvaibleStocksEntity>> GetManufacturersAvailableStocks();

        Task<List<ProductToStockEntity>> GetStockDataForProduct(int product_id);
    }


    public class StockStorage : IStockStorage
    {
        private readonly IDatabaseAccess database;

        public StockStorage(IDatabaseAccess database)
        {
            this.database = database;
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

        public async Task<StockPartnerEntity> GetStockById(int stock_partner_id)
        {
            string sql = @"SELECT sp.*, sc.name as city
                          FROM oc_stock_partner sp
                          LEFT JOIN oc_stock_city sc ON sp.city_id = sc.city_id
                          WHERE sp.stock_partner_id = @stock_partner_id";

            var stock = await database.GetFirstOrDefault<StockPartnerEntity, dynamic>(sql, new { stock_partner_id });

            return stock;
        }

        public async Task CreateOrUpdateStock(StockPartnerEntity stock)
        {
            if (stock.city_id == -1)
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

        public async Task<List<ManufacturerAvaibleStocksEntity>> GetManufacturersAvailableStocks()
        {
            string sql = @"SELECT m.manufacturer_id, GROUP_CONCAT(DISTINCT sp.stock_partner_id SEPARATOR ',') as stock_ids
                            FROM oc_product_to_stock pts 
                            JOIN oc_stock_partner sp ON (sp.stock_partner_id = pts.stock_partner_id) 
                            JOIN oc_product p ON (p.product_id = pts.product_id)
                            JOIN oc_manufacturer m ON (p.manufacturer_id = m.manufacturer_id)
                            GROUP BY p.manufacturer_id
                            ORDER BY m.name;";

            var stocksInfo = await database.GetList<ManufacturerAvaibleStocksEntity>(sql);

            return stocksInfo;

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

        public async Task<List<ProductToStockEntity>> GetStockDataForProduct(int product_id)
        {
            string sql = @"SELECT * FROM oc_product_to_stock WHERE product_id = @product_id";

            var list = await database.GetList<ProductToStockEntity, dynamic>(sql, new { product_id });

            return list;
        }
    }





}
