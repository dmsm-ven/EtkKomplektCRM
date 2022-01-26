using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IManufacturerStorage
    {
        Task<List<ManufacturerEntity>> GetManufacturers();       
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
}
}
