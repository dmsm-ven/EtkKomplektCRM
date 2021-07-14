using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IManufacturerStorage
    {
        Task SaveManufacturer(ManufacturerEntity manufacturer);
        Task<List<ManufacturerEntity>> GetManufacturers();

        Task SaveStockPartner(StockPartnerEntity stock);
        Task<List<StockPartnerEntity>> GetStockPartners();
        Task CreateStock(StockPartnerEntity stock);
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
    }
}
