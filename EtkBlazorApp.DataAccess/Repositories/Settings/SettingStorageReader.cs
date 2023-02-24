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

    public class SettingStorageReader : ISettingStorageReader
    {
        private readonly IDatabaseAccess database;

        public SettingStorageReader(IDatabaseAccess database)
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
            catch
            {

            }
            return default;
        }


    }
}
