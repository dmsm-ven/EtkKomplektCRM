using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IPrikatTemplateStorage
    {
        Task<List<PrikatReportTemplateEntity>> GetPrikatTemplates();
        Task SavePrikatTemplate(PrikatReportTemplateEntity template);
    }

    public class PrikatTemplateStorage : IPrikatTemplateStorage
    {
        private readonly IDatabaseAccess database;

        public PrikatTemplateStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<PrikatReportTemplateEntity>> GetPrikatTemplates()
        {
            string sql = @"SELECT t.*, m.manufacturer_id as manufacturer_id,  m.name as manufacturer_name
                           FROM etk_app_prikat_template t
                           RIGHT JOIN oc_manufacturer m ON t.manufacturer_id = m.manufacturer_id";

            var templatesInfo = await database.GetList<PrikatReportTemplateEntity, dynamic>(sql, new { });
            return templatesInfo;
        }

        public async Task SavePrikatTemplate(PrikatReportTemplateEntity template)
        {
            var sql = @"INSERT INTO etk_app_prikat_template
                       (manufacturer_id, enabled, discount1, discount2, currency_code)
                       VALUES
                       (@manufacturer_id, @enabled, @discount1, @discount2, @currency_code)
                       ON DUPLICATE KEY UPDATE
                       enabled = @enabled,
                       discount1 = @discount1,
                       discount2 = @discount2,
                       currency_code = @currency_code";


            await database.ExecuteQuery(sql, template);
        }

    }
}
