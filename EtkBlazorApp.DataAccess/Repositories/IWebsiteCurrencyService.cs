using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IWebsiteCurrencyService
    {
        Task<List<WebsiteCurrencyStatusEntity>> GetStatus();
    }

    public class WebsiteCurrencyService : IWebsiteCurrencyService
    {
        private readonly IDatabaseAccess database;

        public WebsiteCurrencyService(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<WebsiteCurrencyStatusEntity>> GetStatus()
        {
            string sql = "SELECT value_official, code, date_modified FROM oc_currency";
            var data = await database.GetList<WebsiteCurrencyStatusEntity>(sql);
            return data;
        }
    }
}
