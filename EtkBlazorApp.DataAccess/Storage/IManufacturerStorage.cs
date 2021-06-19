using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IManufacturerStorage
    {
        Task SaveManufacturer(ManufacturerEntity manufacturer);
        Task<List<ManufacturerEntity>> GetManufacturers();

        //Для таблицы монобрендов (для обновления других сайтов) - нужно вынести в отдельный интерфейс
        Task<List<MonobrandEntity>> GetMonobrands();
        Task UpdateMonobrand(MonobrandEntity monobrand);
        Task DeleteMonobrand(int id);
        Task AddMonobrand();
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
    
        public async Task<List<MonobrandEntity>> GetMonobrands()
        {
            string sql = @"SELECT etk_app_monobrand.*, oc_manufacturer.name as manufacturer_name
                         FROM etk_app_monobrand
                         LEFT JOIN oc_manufacturer ON oc_manufacturer.manufacturer_id = etk_app_monobrand.manufacturer_id";
            var monobrands = await database.GetList<MonobrandEntity, dynamic>(sql, new { });

            return monobrands;
        }

        public async Task UpdateMonobrand(MonobrandEntity monobrand)
        {
            string sql = @"UPDATE etk_app_monobrand 
                            SET manufacturer_id = @manufacturer_id,
                                website = @website,
                                currency_code = @currency_code
                                WHERE monobrand_id = @monobrand_id";

            await database.ExecuteQuery(sql, monobrand);
        }

        public async Task DeleteMonobrand(int monobrand_id)
        {
            string sql = "DELETE FROM etk_app_monobrand WHERE monobrand_id = @monobrand_id";

            await database.ExecuteQuery<dynamic>(sql, new { monobrand_id });
        }

        public async Task AddMonobrand()
        {
            await database.ExecuteQuery("INSERT INTO etk_app_monobrand (website, currency_code) VALUES ('https://', 'RUB')");
        }
    }
}
