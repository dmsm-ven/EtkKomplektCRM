using Dapper;
using EtkBlazorApp.DataAccess.Model;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public async Task SaveManufacturer(ManufacturerEntity manufacturer)
        {
            string sql = "UPDATE oc_manufacturer SET shipment_period = @shipment_period WHERE manufacturer_id = @manufacturer_id";
            await SaveData(sql, manufacturer);
        }

        public async Task<List<ManufacturerEntity>> GetManufacturers()
        {
            string sql = "SELECT m.*, url.keyword FROM oc_manufacturer m LEFT JOIN oc_url_alias url ON CONCAT('manufacturer_id=', m.manufacturer_id) = url.query ORDER BY name";
            var manufacturers = await LoadData<ManufacturerEntity, dynamic>(sql, new { });
            return manufacturers;
        }

        public async Task<List<OrderEntity>> GetLastOrders(int takeCount, string city = null)
        {
            if(takeCount <= 0 || takeCount >= 500)
            {
                return new List<OrderEntity>();
            }

            var sb = new StringBuilder()
                .AppendLine("SELECT o.*, s.name as order_status")
                .AppendLine("FROM oc_order o")
                .AppendLine("LEFT JOIN oc_order_status s ON o.order_status_id = s.order_status_id");

            if (!string.IsNullOrWhiteSpace(city))
            {
                sb.AppendLine("WHERE o.payment_city = @City");
            }
            sb
                .AppendLine("ORDER BY o.date_added DESC")
                .AppendLine("LIMIT @TakeCount");

            string sql = sb.ToString().Trim();
            var orders = await LoadData<OrderEntity, dynamic>(sql, new { TakeCount = takeCount, City = city});
            return orders;
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

        public async Task SaveShopAccount(ShopAccountEntity account)
        {
            var sb = new StringBuilder();

            if (account.website_id != 0)
            {
                sb
                .AppendLine("UPDATE etk_app_shop_account")
                .AppendLine($"SET {nameof(account.title)} = @{nameof(account.title)},")
                .AppendLine($"    {nameof(account.uri)} = @{nameof(account.uri)},")
                .AppendLine($"    {nameof(account.ftp_host)} = @{nameof(account.ftp_host)},")
                .AppendLine($"    {nameof(account.ftp_login)} = @{nameof(account.ftp_login)},")
                .AppendLine($"    {nameof(account.ftp_password)} = @{nameof(account.ftp_password)},")
                .AppendLine($"    {nameof(account.db_host)} = @{nameof(account.db_host)},")
                .AppendLine($"    {nameof(account.db_login)} = @{nameof(account.db_login)},")
                .AppendLine($"    {nameof(account.db_password)} = @{nameof(account.db_password)}")
                .AppendLine($"WHERE {nameof(ShopAccountEntity.website_id)} = @{nameof(ShopAccountEntity.website_id)}");
            }
            else
            {
                sb
                    .AppendLine("INSERT INTO etk_app_shop_account (")
                    .AppendLine($"{nameof(account.title)},")
                    .AppendLine($"{nameof(account.uri)},")
                    .AppendLine($"{nameof(account.ftp_host)},")
                    .AppendLine($"{nameof(account.ftp_login)},")
                    .AppendLine($"{nameof(account.ftp_password)},")
                    .AppendLine($"{nameof(account.db_host)},")
                    .AppendLine($"{nameof(account.db_login)},")
                    .AppendLine($"{nameof(account.db_password)}) VALUES (")

                    .AppendLine($"@{nameof(account.title)},")
                    .AppendLine($"@{nameof(account.uri)},")
                    .AppendLine($"@{nameof(account.ftp_host)},")
                    .AppendLine($"@{nameof(account.ftp_login)},")
                    .AppendLine($"@{nameof(account.ftp_password)},")
                    .AppendLine($"@{nameof(account.db_host)},")
                    .AppendLine($"@{nameof(account.db_login)},")
                    .AppendLine($"@{nameof(account.db_password)})");
            }

            string sql = sb.ToString().Trim();
            await SaveData(sql, account);

            account.website_id = await GetScalar<int, dynamic>($"SELECT max({nameof(account.website_id)}) FROM etk_app_shop_account", new { });
        }

        public async Task<List<ShopAccountEntity>> GetShopAccounts()
        {
            var sql = "SELECT * FROM etk_app_shop_account";
            var data = await LoadData<ShopAccountEntity, dynamic>(sql, new { });
            return data;
        }

        public async Task DeleteShopAccounts(int id)
        {
            var sql = $"DELETE FROM etk_app_shop_account WHERE {nameof(ShopAccountEntity.website_id)} = @{nameof(ShopAccountEntity.website_id)}";
            await SaveData<dynamic>(sql, new { website_id = id });
        }

        #region private
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
        #endregion
    }
}
