using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class MySqlDatabase : IDatabase
    {
        private readonly IConfiguration configuration;
        string ConnectionString => configuration.GetConnectionString("openserver_etk_db");

        public MySqlDatabase(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private async Task<List<T>> LoadData<T, U>(string sql, U parameters, string connectionString)
        {
            using (IDbConnection connection = new MySqlConnection(connectionString))
            {
                var rows = await connection.QueryAsync<T>(sql, parameters);

                return rows.ToList();
            }
        }

        private Task SaveData<T>(string sql, T parameters, string connectionString)
        {
            using (IDbConnection connection = new MySqlConnection(connectionString))
            {
                return connection.ExecuteAsync(sql, parameters);
            }
        }

        public async Task SaveManufacturer(ManufacturerModel manufacturer)
        {
            string sql = "UPDATE oc_manufacturer SET shipment_period = @shipment_period WHERE manufacturer_id = @manufacturer_id";
            await SaveData(sql, manufacturer, ConnectionString);
        }

        public async Task<List<ManufacturerModel>> GetManufacturers()
        {
            string sql = "SELECT m.*, url.keyword FROM oc_manufacturer m LEFT JOIN oc_url_alias url ON CONCAT('manufacturer_id=', m.manufacturer_id) = url.query ORDER BY name";
            var manufacturers = await LoadData<ManufacturerModel, dynamic>(sql, new { }, ConnectionString);
            return manufacturers;
        }
   
        public async Task<string> GetUserPremission(string login, string password)
        {
            return "administrator";
        }
    }
}
