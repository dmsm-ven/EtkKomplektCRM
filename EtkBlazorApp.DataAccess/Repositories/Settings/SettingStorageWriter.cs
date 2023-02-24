using System.ComponentModel;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public class SettingStorageWriter : ISettingStorageWriter
    {
        private readonly IDatabaseAccess database;

        public SettingStorageWriter(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task SetValue(string name, string value)
        {
            string checkSql = "SELECT COUNT(*) FROM etk_app_setting WHERE name = @name";

            bool recordExists = (await database.GetScalar<int, dynamic>(checkSql, new { name })) > 0;

            value = value ?? string.Empty;

            if (recordExists)
            {
                string updateQuery = $"UPDATE etk_app_setting SET value = @value WHERE name = @name";
                await database.ExecuteQuery<dynamic>(updateQuery, new { name, value });
            }
            else
            {
                var insertQuery = $"INSERT INTO etk_app_setting (name, value) VALUES (@name, @value)";
                await database.ExecuteQuery<dynamic>(insertQuery, new { name, value });
            }
        }

        public async Task SetValueToDateTimeNow(string name)
        {
            string checkSql = "SELECT COUNT(*) FROM etk_app_setting WHERE name = @name";

            bool recordExists = (await database.GetScalar<int, dynamic>(checkSql, new { name })) > 0;

            if (recordExists)
            {
                string updateQuery = $"UPDATE etk_app_setting SET value = NOW() WHERE name = @name";
                await database.ExecuteQuery<dynamic>(updateQuery, new { name });
            }
            else
            {
                var insertQuery = $"INSERT INTO etk_app_setting (name, value) VALUES (@name, NOW())";
                await database.ExecuteQuery<dynamic>(insertQuery, new { name });
            }
        }

        public async Task SetValue<T>(string name, T value)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                var typeConvertedStringValue = converter.ConvertToInvariantString(value);
                await SetValue(name, typeConvertedStringValue);
            }
            catch
            {
                throw;
            }
        }

    }
}
