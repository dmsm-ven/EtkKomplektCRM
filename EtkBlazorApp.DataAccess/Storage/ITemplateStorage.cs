using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ITemplateStorage
    {
        Task<List<PriceListTemplateEntity>> GetPriceListTemplates();
        Task<List<PrikatReportTemplateEntity>> GetPrikatTemplates();
        Task SavePrikatTemplate(PrikatReportTemplateEntity template);
    }

    public class TemplateStorage : ITemplateStorage
    {
        private readonly IDatabaseAccess database;

        public TemplateStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<PriceListTemplateEntity>> GetPriceListTemplates()
        {
            string sql = "SELECT * FROM etk_app_price_list_template";
            var templatesInfo = await database.LoadData<PriceListTemplateEntity, dynamic>(sql, new { });
            return templatesInfo;
        }

        public async Task<List<PrikatReportTemplateEntity>> GetPrikatTemplates()
        {
            string sql = "SELECT t.*, m.name as manufacturer_name FROM etk_app_prikat_template t " +
                         "LEFT JOIN oc_manufacturer m ON t.manufacturer_id = m.manufacturer_id";

            var templatesInfo = await database.LoadData<PrikatReportTemplateEntity, dynamic>(sql, new { });
            return templatesInfo;
        }

        public async Task SavePrikatTemplate(PrikatReportTemplateEntity template)
        {
            string sql = "UPDATE etk_app_prikat_template " +
                         "SET discount1 = @discount1, " +
                             "discount2 = @discount2, " +
                             "status = @status, " +
                             "currency_code = @currency_code " +
                         "WHERE manufacturer_id = @manufacturer_id";

            await database.SaveData(sql, template);
        }
    }
}
