using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IMonobrandStorage
    {
        //Для таблицы монобрендов (для обновления других сайтов) - нужно вынести в отдельный интерфейс
        Task<List<MonobrandEntity>> GetMonobrands();
        Task UpdateMonobrand(MonobrandEntity monobrand);
        Task DeleteMonobrand(int id);
        Task AddMonobrand();
    }

    public class MonobrandStorage : IMonobrandStorage
    {
        private readonly IDatabaseAccess database;

        public MonobrandStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<MonobrandEntity>> GetMonobrands()
        {
            string sql = @"SELECT etk_app_monobrand.*, oc_manufacturer.name as manufacturer_name
                         FROM etk_app_monobrand
                         LEFT JOIN oc_manufacturer ON oc_manufacturer.manufacturer_id = etk_app_monobrand.manufacturer_id";
            var monobrands = await database.GetList<MonobrandEntity, dynamic>(sql, new { });

            return monobrands;
        }

        public async Task UpdateMonobrand(MonobrandEntity monobrand)
        {
            string sql = @"UPDATE etk_app_monobrand 
                            SET manufacturer_id = @manufacturer_id,
                                website = @website,
                                currency_code = @currency_code
                                WHERE monobrand_id = @monobrand_id";

            await database.ExecuteQuery(sql, monobrand);
        }

        public async Task DeleteMonobrand(int monobrand_id)
        {
            string sql = "DELETE FROM etk_app_monobrand WHERE monobrand_id = @monobrand_id";

            await database.ExecuteQuery<dynamic>(sql, new { monobrand_id });
        }

        public async Task AddMonobrand()
        {
            await database.ExecuteQuery("INSERT INTO etk_app_monobrand (website, currency_code) VALUES ('https://', 'RUB')");
        }
    }
}
