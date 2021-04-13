using EtkBlazorApp.DataAccess.Entity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ISettingStorage
    {
        public Task<string> GetValue(string name);
        public Task<T> GetValue<T>(string name);

        public Task SetValue(string name, string value);       
        public Task SetValue<T>(string name, T value);
        public Task SetValueToDateTimeNow(string name);

        public Task<List<CronTaskEntity>> GetCronTasks();
        public Task<CronTaskEntity> GetCronTaskById(int id);
        public Task UpdateCronTask(CronTaskEntity task);
    }

    public class SettingStorage : ISettingStorage
    {
        private readonly IDatabaseAccess database;

        public SettingStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<string> GetValue(string name)
        {
            var sql = $"SELECT value FROM etk_app_setting WHERE name = @name";
            string result = await database.GetScalar<string, dynamic>(sql, new { name }) ?? string.Empty;
            return result;
        }

        public async Task<T> GetValue<T>(string name)
        {
            // TODO добавить сюда и в SetValue проверку: 
            // если тип сложный класс то выполнять json сериализацию/десериализацию
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                var stringValue = await GetValue(name);
                var value = (T)(converter.ConvertFromInvariantString(stringValue));
                return value;
            }
            catch(Exception ex)
            {

            }
            return default;
        }

        public async Task SetValue(string name, string value)
        {
            string checkSql = "SELECT COUNT(*) FROM etk_app_setting WHERE name = @name";

            bool recordExists = (await database.GetScalar<int, dynamic>(checkSql, new { name })) > 0;

            value = value ?? string.Empty;

            if (recordExists)
            {
                string updateQuery = $"UPDATE etk_app_setting SET value = @value WHERE name = @name";
                await database.SaveData<dynamic>(updateQuery, new { name, value });
            }
            else
            {
                var insertQuery = $"INSERT INTO etk_app_setting (name, value) VALUES (@name, @value)";
                await database.SaveData<dynamic>(insertQuery, new { name, value });
            }         
        }

        public async Task SetValueToDateTimeNow(string name)
        {
            string checkSql = "SELECT COUNT(*) FROM etk_app_setting WHERE name = @name";

            bool recordExists = (await database.GetScalar<int, dynamic>(checkSql, new { name })) > 0;

            if (recordExists)
            {
                string updateQuery = $"UPDATE etk_app_setting SET value = NOW() WHERE name = @name";
                await database.SaveData<dynamic>(updateQuery, new { name });
            }
            else
            {
                var insertQuery = $"INSERT INTO etk_app_setting (name, value) VALUES (@name, NOW())";
                await database.SaveData<dynamic>(insertQuery, new { name });
            }
        }

        public async Task SetValue<T>(string name, T value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                var typeConvertedStringValue = converter.ConvertToInvariantString(value);
                await SetValue(name, typeConvertedStringValue);
            }
            catch(Exception ex)
            {

            }
        }    
    
        public async Task<List<CronTaskEntity>> GetCronTasks()
        {
            string sql = "SELECT * FROM etk_app_cron_task";

            var data = await database.LoadData<CronTaskEntity, dynamic>(sql, new { });

            return data;
        }

        public async Task<CronTaskEntity> GetCronTaskById(int id)
        {
            string sql = "SELECT * FROM etk_app_cron_task WHERE task_id = @id";

            var data = await database.LoadData<CronTaskEntity, dynamic>(sql, new { id });

            return data.FirstOrDefault();
        }

        public async Task UpdateCronTask(CronTaskEntity task)
        {
            string sql = "UPDATE etk_app_cron_task SET " +
                                                "enabled = @enabled, " +
                                                "exec_time = @exec_time, " +
                                                "last_exec_date_time = @last_exec_date_time " +
                                                "WHERE task_id = @task_id";

            await database.SaveData(sql, task);
        }


    }
}
