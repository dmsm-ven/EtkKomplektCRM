using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IManufacturerStorage
    {
        Task SaveManufacturer(ManufacturerEntity manufacturer);
        Task<List<ManufacturerEntity>> GetManufacturers();
        Task<List<MonobrandEntity>> GetMonobrands();
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
            string sql = "UPDATE oc_manufacturer " +
                         "SET shipment_period = @shipment_period " +
                         "WHERE manufacturer_id = @manufacturer_id";
            await database.SaveData(sql, manufacturer);
        }

        public async Task<List<ManufacturerEntity>> GetManufacturers()
        {
            string sql = "SELECT m.*, url.keyword " +
                         "FROM oc_manufacturer m " +
                         "LEFT JOIN oc_url_alias url ON CONCAT('manufacturer_id=', m.manufacturer_id) = url.query " +
                         "ORDER BY name";
            var manufacturers = await database.LoadData<ManufacturerEntity, dynamic>(sql, new { });
            return manufacturers;
        }            
    
        public async Task<List<MonobrandEntity>> GetMonobrands()
        {
            string sql = "SELECT etk_app_monobrand.*, oc_manufacturer.name as manufacturer_name " +
                "FROM etk_app_monobrand " +
                "LEFT JOIN oc_manufacturer ON oc_manufacturer.manufacturer_id = etk_app_monobrand.manufacturer_id";
            var monobrands = await database.LoadData<MonobrandEntity, dynamic>(sql, new { });

            return monobrands;
        }
    }
}
