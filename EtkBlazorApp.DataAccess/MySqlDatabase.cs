using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        private async Task<List<T>> LoadData<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new MySqlConnection(ConnectionString))
            {
                var rows = await connection.QueryAsync<T>(sql, parameters);

                return rows.ToList();
            }
        }

        private async Task<T> GetScalar<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new MySqlConnection(ConnectionString))
            {
                T value = await connection.ExecuteScalarAsync<T>(sql, parameters);

                return value;
            }
        }

        private Task SaveData<T>(string sql, T parameters)
        {
            using (IDbConnection connection = new MySqlConnection(ConnectionString))
            {
                return connection.ExecuteAsync(sql, parameters);
            }
        }

        public async Task SaveManufacturer(ManufacturerModel manufacturer)
        {
            string sql = "UPDATE oc_manufacturer SET shipment_period = @shipment_period WHERE manufacturer_id = @manufacturer_id";
            await SaveData(sql, manufacturer);
        }

        public async Task<List<ManufacturerModel>> GetManufacturers()
        {
            string sql = "SELECT m.*, url.keyword FROM oc_manufacturer m LEFT JOIN oc_url_alias url ON CONCAT('manufacturer_id=', m.manufacturer_id) = url.query ORDER BY name";
            var manufacturers = await LoadData<ManufacturerModel, dynamic>(sql, new { });
            return manufacturers;
        }
   
        public async Task<bool> GetUserPremission(string login, string password)
        {
            var sb = new StringBuilder()
                .AppendLine("SELECT COUNT(user_id) FROM oc_user")
                .AppendLine("WHERE user_group_id = (SELECT user_group_id FROM oc_user_group WHERE name = 'etk_app')")
                .AppendLine("AND username = @login AND password = @password")
                .AppendLine("AND status = 1");

            var sql = sb.ToString().Trim();

            string passwordMd5 = CreateFromStringMD5(password);
            var result = await GetScalar<int, dynamic>(sql, new { login, password = passwordMd5 });

            return result == 1;
        }

        private string CreateFromStringMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
