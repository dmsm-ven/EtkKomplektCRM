using EtkBlazorApp.DataAccess.Entity;
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
        Task AddOrUpdateSingleProductDiscount(int product_id, decimal discount_percent);
        Task RemoveSingleProductDiscount(int product_id);
        Task<List<ProductEntity>> GetDiscountedProducts();
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
                       (manufacturer_id, enabled, discount, currency_code, checked_stocks)
                       VALUES
                       (@manufacturer_id, @enabled, @discount, @currency_code, @checked_stocks)";
                await database.ExecuteQuery(insertSql, template);

                template.template_id = await database.GetScalar<int>("SELECT max(template_id) FROM etk_app_prikat_template");
            }
            else
            {
                string updateSql = @"UPDATE etk_app_prikat_template
                                    SET 
                                        manufacturer_id = @manufacturer_id,
                                        enabled = @enabled,
                                        discount = @discount,
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
                    discount = 0,
                    checked_stocks = "",
                    currency_code = "RUB",
                    enabled = true
                });
            }
        }

        public async Task RemoveSingleProductDiscount(int product_id)
        {
            string deleteSql = @"DELETE FROM etk_app_prikat_product_discount WHERE product_id = @product_id";
            await database.ExecuteQuery(deleteSql, new { product_id });
        }

        public async Task AddOrUpdateSingleProductDiscount(int product_id, decimal discount_percent)
        {
            string insertSql = @"INSERT INTO etk_app_prikat_product_discount (product_id, discount) VALUES 
                       (@product_id, @discount_percent)
                        ON DUPLICATE KEY UPDATE discount = @discount_percent";
            await database.ExecuteQuery(insertSql, new { product_id, discount_percent });
        }

        public async Task<List<ProductEntity>> GetDiscountedProducts()
        {
            string sql = @"SELECT pd.product_id, d.name, pd.discount as discount_price
                           FROM etk_app_prikat_product_discount pd
                           JOIN oc_product_description d ON (pd.product_id = d.product_id)";

            var products = await database.GetList<ProductEntity>(sql);

            return products;
        }
    }
}
