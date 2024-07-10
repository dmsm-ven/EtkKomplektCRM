using EtkBlazorApp.DataAccess.Entity.Marketplace;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Repositories
{
    public interface IPrikatTemplateStorage
    {
        Task<List<PrikatReportTemplateEntity>> GetPrikatTemplates(bool includeDisabled);
        Task SavePrikatTemplate(PrikatReportTemplateEntity template);
        Task DisablePrikatTemplate(int template_id);
        Task AddNewOrRestorePrikatTemplate(int manufacturer_id);
    }

    public class PrikatTemplateStorage : IPrikatTemplateStorage
    {
        private readonly IDatabaseAccess database;

        public PrikatTemplateStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<PrikatReportTemplateEntity>> GetPrikatTemplates(bool includeDisabled)
        {
            string sql = @"SELECT t.*, m.name as manufacturer_name
                           FROM etk_app_prikat_template t
                           JOIN oc_manufacturer m ON t.manufacturer_id = m.manufacturer_id
                           WHERE enabled = 1 OR @includeDisabled
                           ORDER BY m.name";

            var templatesInfo = await database.GetList<PrikatReportTemplateEntity, dynamic>(sql, new { includeDisabled });
            return templatesInfo;
        }

        public async Task DisablePrikatTemplate(int template_id)
        {
            string sql = @"UPDATE etk_app_prikat_template SET enabled = 0 WHERE template_id = @template_id";
            await database.ExecuteQuery(sql, new { template_id });
        }

        public async Task SavePrikatTemplate(PrikatReportTemplateEntity template)
        {
            if (template.template_id == 0)
            {
                string insertSql = @"INSERT INTO etk_app_prikat_template
                       (manufacturer_id, enabled, discount1, discount2, currency_code, checked_stocks)
                       VALUES
                       (@manufacturer_id, @enabled, @discount1, @discount2, @currency_code, @checked_stocks)";
                await database.ExecuteQuery(insertSql, template);

                template.template_id = await database.GetScalar<int>("SELECT max(template_id) FROM etk_app_prikat_template");
            }
            else
            {
                string updateSql = @"UPDATE etk_app_prikat_template
                                    SET 
                                        manufacturer_id = @manufacturer_id,
                                        enabled = @enabled,
                                        discount1 = @discount1,
                                        discount2 = @discount2,
                                        currency_code = @currency_code,
                                        checked_stocks = @checked_stocks
                                    WHERE template_id = @template_id";

                await database.ExecuteQuery(updateSql, template);
            }
        }

        public async Task AddNewOrRestorePrikatTemplate(int manufacturer_id)
        {
            var allTemplates = await GetPrikatTemplates(includeDisabled: true);
            var existedTemplate = allTemplates.FirstOrDefault(t => t.manufacturer_id == manufacturer_id);
            if (existedTemplate != null)
            {
                existedTemplate.enabled = true;
                await SavePrikatTemplate(existedTemplate);
            }
            else
            {
                await SavePrikatTemplate(new PrikatReportTemplateEntity()
                {
                    manufacturer_id = manufacturer_id,
                    discount1 = 0,
                    discount2 = 0,
                    checked_stocks = "",
                    currency_code = "RUB",
                    enabled = true
                });
            }
        }
    }
}
