using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IPriceListTemplateStorage
    {
        Task<List<PriceListTemplateEntity>> GetTemplates();
    }

    public class PriceListTemplateStorage : IPriceListTemplateStorage
    {
        private readonly IDatabaseAccess database;

        public PriceListTemplateStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<PriceListTemplateEntity>> GetTemplates()
        {
            string sql = "SELECT * FROM etk_app_price_list_template";
            var templatesInfo = await database.LoadData<PriceListTemplateEntity, dynamic>(sql, new { });
            return templatesInfo;
        }
    }
}
