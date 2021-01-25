using EtkBlazorApp.DataAccess.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ISettingStorage
    {
        Task SaveShopAccount(ShopAccountEntity shopAccount);
        Task<List<ShopAccountEntity>> GetShopAccounts();
        Task DeleteShopAccounts(int id);

        Task<Dictionary<int, int>> GetOzonDiscounts();
        Task SetOzonDiscounts(string data);
        Task<List<int>> GetOzonCheckedManufacturers();
        Task SetOzonCheckedManufacturers(string data);

        Task<Dictionary<int, Tuple<int, int>>> GetPrikatDiscounts();
        Task SetPrikatDiscounts(string data);
        Task<List<int>> GetPrikatCheckedManufacturers();
        Task SetPrikatCheckedManufacturers(string data);
    }

    public class SettingStorage : ISettingStorage
    {
        private readonly IDatabaseAccess database;
        private readonly IConfiguration configration;
        JsonSerializerSettings jsonSettings;

        public SettingStorage(IDatabaseAccess database, IConfiguration configuration)
        {
            this.database = database;
            this.configration = configuration;

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.Formatting = Formatting.None;
            jsonSettings.ContractResolver = new EncryptedStringPropertyResolver(configuration.GetSection("settings_encrypt_key").Value);
        }

        public async Task SaveShopAccount(ShopAccountEntity account)
        {
            if (account.website_id == 0)
            {
                await database.SaveData<dynamic>("INSERT INTO etk_app_monobrand (account) VALUES ('')", new { });
                account.website_id = await database.GetScalar<int, dynamic>($"SELECT max({nameof(account.website_id)}) FROM etk_app_monobrand", new { });
            }

            var sb = new StringBuilder()
                .AppendLine("UPDATE etk_app_monobrand")
                .AppendLine($"SET account = @data")
                .AppendLine($"WHERE {nameof(ShopAccountEntity.website_id)} = @{nameof(ShopAccountEntity.website_id)}");

            string jsonData = JsonConvert.SerializeObject(account, jsonSettings);
            string sql = sb.ToString().Trim();
            await database.SaveData(sql, new { data = jsonData, website_id = account.website_id });
        }

        public async Task<List<ShopAccountEntity>> GetShopAccounts()
        {
            var sql = "SELECT * FROM etk_app_monobrand";
            var encryptedData = await database.LoadData<dynamic, dynamic>(sql, new { });
            var decruptedData = encryptedData.Select(item => (ShopAccountEntity)JsonConvert.DeserializeObject<ShopAccountEntity>(item.account, jsonSettings)).ToList();
            return decruptedData;
        }

        public async Task DeleteShopAccounts(int id)
        {
            var sql = $"DELETE FROM etk_app_monobrand WHERE {nameof(ShopAccountEntity.website_id)} = @{nameof(ShopAccountEntity.website_id)}";
            await database.SaveData<dynamic>(sql, new { website_id = id });
        }

        public async Task<Dictionary<int,int>> GetOzonDiscounts()
        {
            var sql = $"SELECT value FROM etk_app_setting WHERE name = @settingsName";
            var result = await database.GetScalar<string, dynamic>(sql, new { settingsName = "ozon_manufacturer_discounts" });

            var data = result.Split(';')
                .Select(data => data.Split('='))
                .Select(array => new { manufacturer_id = int.Parse(array[0]), discount = int.Parse(array[1]) })
                .ToDictionary(i => i.manufacturer_id, i => i.discount);


            return data;
        }

        public async Task SetOzonDiscounts(string data)
        {
            var sql = $"UPDATE etk_app_setting SET value = @data WHERE name = @settingsName";
            await database.SaveData<dynamic>(sql, new { data, settingsName = "ozon_manufacturer_discounts" });
        }

        public async Task<Dictionary<int, Tuple<int,int>>> GetPrikatDiscounts()
        {
            var sql = $"SELECT value FROM etk_app_setting WHERE name = @settingsName";
            var result = await database.GetScalar<string, dynamic>(sql, new { settingsName = "prikat_manufacturer_discounts" });

            var data = result.Split(';')
            .Select(data => data.Split('='))
            .Select(array => new
            {
                manufacturer_id = int.Parse(array[0]),
                discount1 = int.Parse(array[1].Split('|')[0]),
                discount2 = int.Parse(array[1].Split('|')[1])
            })
            .ToDictionary(i => i.manufacturer_id, i => Tuple.Create(i.discount1, i.discount2));

            return data;
        }

        public async Task SetPrikatDiscounts(string data)
        {
            var sql = $"UPDATE etk_app_setting SET value = @data WHERE name = @settingsName";
            await database.SaveData<dynamic>(sql, new { data, settingsName = "prikat_manufacturer_discounts" });
        }

        public async Task<List<int>> GetPrikatCheckedManufacturers()
        {
            var sql = $"SELECT value FROM etk_app_setting WHERE name = @settingsName";
            var result = await database.GetScalar<string, dynamic>(sql, new { settingsName = "prikat_checked_manufacturers" });

            if (!string.IsNullOrWhiteSpace(result))
            {
                var data = result.Split(';').Select(i => int.Parse(i)).ToList();

                return data;
            }
            return new List<int>();
        }

        public async Task SetPrikatCheckedManufacturers(string data)
        {
            string settingName = "prikat_checked_manufacturers";
            var sql = $"UPDATE etk_app_setting SET value = @data WHERE name = @settingName";
            await database.SaveData<dynamic>(sql, new { data, settingName });

        }

        public async Task<List<int>> GetOzonCheckedManufacturers()
        {
            var sql = $"SELECT value FROM etk_app_setting WHERE name = @settingsName";
            var result = await database.GetScalar<string, dynamic>(sql, new { settingsName = "ozon_checked_manufacturers" });

            if (!string.IsNullOrWhiteSpace(result))
            {
                var data = result.Split(';').Select(i => int.Parse(i)).ToList();

                return data;
            }
            return new List<int>();
        }

        public async Task SetOzonCheckedManufacturers(string data)
        {
            string settingName = "ozon_checked_manufacturers";
            var sql = $"UPDATE etk_app_setting SET value = @data WHERE name = @settingName";
            await database.SaveData<dynamic>(sql, new { data, settingName });
        }
    }
}
