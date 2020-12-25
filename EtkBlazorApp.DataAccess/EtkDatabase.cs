using Dapper;
using EtkBlazorApp.DataAccess.Model;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class EtkDatabase : IDatabase
    {
        private readonly IConfiguration configuration;
        private readonly JsonSerializerSettings jsonSettings;

        string ConnectionString
        {
            get
            {
#if DEBUG
                return configuration.GetConnectionString("local_server_db");
#else
                return configuration.GetConnectionString("prod_server_db");             
#endif
            }
        }

        public EtkDatabase(IConfiguration configuration)
        {
            this.configuration = configuration;

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.Formatting = Formatting.None;
            jsonSettings.ContractResolver = new EncryptedStringPropertyResolver("0D83C66D-D21C-401B-8F5B-C1E73CB713A0");
        }

        #region Auth
        public async Task<string> GetUserPermission(string login, string password)
        {
            var sb = new StringBuilder()
                .AppendLine("SELECT permission")
                .AppendLine("FROM etk_app_user u")
                .AppendLine("LEFT JOIN etk_app_User_group g ON u.user_group_id = g.user_group_id")
                .AppendLine("WHERE u.status = 1 AND login = @login AND password = @password");

            var sql = sb.ToString().Trim();

            string passwordMd5 = CreateFromStringMD5(password);

            var permission = await GetScalar<string, dynamic>(sql, new { login, password = passwordMd5 });

            return permission;
        }

        public async Task SetUserBadPasswordTryCounter(string ip, int tryCount)
        {
            string sql = "UPDATE etk_app_ban_list SET current_try_counter = @tryCount, last_access = NOW() WHERE ip = @ip";

            await SaveData<dynamic>(sql, new { tryCount, ip });
        }

        public async Task<int> GetUserBadPasswordTryCounter(string ip)
        {
            dynamic param = new { ip };

            var sb = new StringBuilder()
                .AppendLine(" SELECT IF( EXISTS(SELECT * FROM etk_app_ban_list WHERE ip = @ip),")
                .AppendLine("(SELECT current_try_counter FROM etk_app_ban_list WHERE ip = @ip), ")
                .AppendLine("-1)");

            string sql = sb.ToString().Trim();
            int tryCount = await GetScalar<int, dynamic>(sql, param);

            if (tryCount == -1)
            {
                await SaveData<dynamic>("INSERT INTO etk_app_ban_list (ip) VALUES (@ip)", param);
                tryCount = 0;
            }

            return tryCount;


        }
        #endregion

        #region Product

        public async Task<List<ProductEntity>> GetLastAddedProducts(int count)
        {
            if (count > 100)
            {
                throw new ArgumentOutOfRangeException("Превышен предел запрашиваемых товаров (100)");
            }

            var sb = new StringBuilder()
                .AppendLine("SELECT p.*, d.name as name, m.name as manufacturer")
                .AppendLine("FROM oc_product p")
                .AppendLine("LEFT JOIN oc_product_description d ON p.product_id = d.product_id")
                .AppendLine("LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                .AppendLine("ORDER BY Date(p.date_added) DESC")
                .AppendLine("LIMIT @Limit");

            string sql = sb.ToString();

            var products = await LoadData<ProductEntity, dynamic>(sql, new { Limit = count });

            return products.ToList();
        }

        #endregion

        #region Manufacturer
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
        #endregion

        #region ShopAccount
        public async Task SaveShopAccount(ShopAccountEntity account)
        {

            if (account.website_id == 0)
            {
                await SaveData<dynamic>("INSERT INTO etk_app_monobrand (account) VALUES ('')", new { });
                account.website_id = await GetScalar<int, dynamic>($"SELECT max({nameof(account.website_id)}) FROM etk_app_monobrand", new { });
            }

            var sb = new StringBuilder()
                .AppendLine("UPDATE etk_app_monobrand")
                .AppendLine($"SET account = @data")
                .AppendLine($"WHERE {nameof(ShopAccountEntity.website_id)} = @{nameof(ShopAccountEntity.website_id)}");

            string jsonData = JsonConvert.SerializeObject(account, jsonSettings);
            string sql = sb.ToString().Trim();
            await SaveData(sql, new { data = jsonData, website_id = account.website_id });
        }

        public async Task<List<ShopAccountEntity>> GetShopAccounts()
        {
            var sql = "SELECT * FROM etk_app_monobrand";
            var encryptedData = await LoadData<dynamic, dynamic>(sql, new { });
            var decruptedData = encryptedData.Select(item => (ShopAccountEntity)JsonConvert.DeserializeObject<ShopAccountEntity>(item.account, jsonSettings)).ToList();
            return decruptedData;
        }

        public async Task DeleteShopAccounts(int id)
        {
            var sql = $"DELETE FROM etk_app_monobrand WHERE {nameof(ShopAccountEntity.website_id)} = @{nameof(ShopAccountEntity.website_id)}";
            await SaveData<dynamic>(sql, new { website_id = id });
        }
        #endregion

        #region Orders
        public async Task<List<OrderEntity>> GetLastOrders(int takeCount, string city = null)
        {
            if (takeCount <= 0 || takeCount >= 500)
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
            var orders = await LoadData<OrderEntity, dynamic>(sql, new { TakeCount = takeCount, City = city });
            return orders;
        }

        public async Task<List<OrderDetailsEntity>> GetOrderDetails(int orderId)
        {
            string sql = "SELECT op.*, p.sku as sku, m.name as manufacturer FROM oc_order_product op " +
                "LEFT JOIN oc_product p ON op.product_id = p.product_id " +
                "LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id " +
                "WHERE order_id = @Id";

            var details = await LoadData<OrderDetailsEntity, dynamic>(sql, new { Id = orderId });

            return details.ToList();
        }

        public async Task<OrderEntity> GetOrderById(int orderId)
        {
            var sql = $"SELECT * FROM oc_order WHERE order_id = @Id";

            var order = (await LoadData<OrderEntity, dynamic>(sql, new { Id = orderId })).FirstOrDefault();

            return order;
        }
        #endregion

        #region Logging
        public async Task AddLogEntries(IEnumerable<LogEntryEntity> logEntries)
        {
            using (IDbConnection connection = new MySqlConnection(ConnectionString))
            {
                foreach (var entry in logEntries)
                {
                    string sql = $"INSERT INTO etk_app_log (user, group_name, date_time, title, message) VALUES " +
                        $"(@{nameof(entry.user)}, @{nameof(entry.group_name)}, @{nameof(entry.date_time)}, @{nameof(entry.title)}, @{nameof(entry.message)})";

                    await connection.ExecuteAsync(sql, entry);
                }
            }
        }

        public async Task<List<LogEntryEntity>> GetLogItems(int count, int maxDaysOld)
        {
            string sql = $"SELECT * FROM etk_app_log ORDER BY date_time DESC LIMIT @limit";

            if (maxDaysOld > 0)
            {
                maxDaysOld *= -1;
                sql = sql.Insert(sql.IndexOf("ORDER BY"), "WHERE DATE(date_time) >= ADDDATE(NOW(), INTERVAL @maxDaysOld DAY) ");
            }
            else if (maxDaysOld < 0)
            {
                sql = sql.Insert(sql.IndexOf("ORDER BY"), "WHERE DATE(date_time) = DATE(ADDDATE(NOW(), INTERVAL @maxDaysOld DAY)) ");
            }
            var data = await LoadData<LogEntryEntity, dynamic>(sql, new { limit = count, maxDaysOld });
            return data.ToList();
        }
        #endregion

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
