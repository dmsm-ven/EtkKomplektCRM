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
        Task CreatePriceList(PriceListTemplateEntity data);
        Task UpdatePriceList(PriceListTemplateEntity data);
        Task<List<PriceListTemplateEntity>> GetPriceListTemplates();
        Task<PriceListTemplateEntity> GetPriceListTemplateById(string guid);
        Task DeletePriceList(string guid);
        Task ChangePriceListTemplateDiscount(string guid, decimal discount);
        Task<List<PriceListTemplateRemoteUriMethodEntity>> GetPricelistTemplateRemoteLoadMethods();
        Task<List<PriceListTemplateContentTypeEntity>> GetPriceListTemplateContentTypes();
        Task<List<string>> GetPriceListTemplatGroupNames();

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
            string sql = @"SELECT t.*, ct.name as content_type_name, lm.name as remote_uri_method_name
                          FROM etk_app_price_list_template t
                          LEFT JOIN etk_app_price_list_template_content_type ct ON t.content_type_id = ct.id
                          LEFT JOIN etk_app_price_list_template_load_method lm ON t.remote_uri_method_id = lm.id";

            var templatesInfo = await database.GetList<PriceListTemplateEntity, dynamic>(sql, new { });
            return templatesInfo;
        }

        public async Task<PriceListTemplateEntity> GetPriceListTemplateById(string guid)
        {
            string sql = @"SELECT t.*, ct.name as content_type_name, lm.name as remote_uri_method_name
                          FROM etk_app_price_list_template t
                          LEFT JOIN etk_app_price_list_template_content_type ct ON t.content_type_id = ct.id
                          LEFT JOIN etk_app_price_list_template_load_method lm ON t.remote_uri_method_id = lm.id 
                          WHERE t.id = @guid LIMIT 1";

            var templateInfo = (await database.GetList<PriceListTemplateEntity, dynamic>(sql, new { guid })).FirstOrDefault();
            return templateInfo;
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

        public async Task ChangePriceListTemplateDiscount(string id, decimal discount)
        {
            string sql = "UPDATE etk_app_price_list_template SET discount = @discount WHERE id = @id";
            await database.ExecuteQuery(sql, new { id, discount});
        }

        public async Task<List<PriceListTemplateRemoteUriMethodEntity>> GetPricelistTemplateRemoteLoadMethods()
        {
            string sql = "SELECT * FROM etk_app_price_list_template_load_method";
            var data = await database.GetList<PriceListTemplateRemoteUriMethodEntity>(sql);
            return data;
        }

        public async Task<List<PriceListTemplateContentTypeEntity>> GetPriceListTemplateContentTypes()
        {
            string sql = "SELECT * FROM etk_app_price_list_template_content_type";
            var data = await database.GetList<PriceListTemplateContentTypeEntity>(sql);
            return data;
        }

        public async Task<List<string>> GetPriceListTemplatGroupNames()
        {
            string sql = "SELECT DISTINCT group_name FROM etk_app_price_list_template ORDER BY group_name";
            var data = await database.GetList<string>(sql);
            return data;
        }

        public async Task UpdatePriceList(PriceListTemplateEntity data)
        {
            string sql = @"UPDATE etk_app_price_list_template
                            SET id = @id,
                                title = @title,
                                description = @description,
                                group_name = @group_name,
                                content_type_id = @content_type_id,
                                remote_uri = @remote_uri,
                                remote_uri_method_id = @remote_uri_method_id,
                                nds = @nds,
                                discount = @discount,
                                image = @image
                            WHERE id = @id";

            await database.ExecuteQuery(sql, data);
        }

        public async Task CreatePriceList(PriceListTemplateEntity data)
        {          
            string sql = @"INSERT INTO etk_app_price_list_template
                        (id, title, description, group_name, content_type_id, remote_uri, remote_uri_method_id, nds, discount, image) VALUES
                        (@id, @title, @description, @group_name, @content_type_id, @remote_uri, @remote_uri_method_id, @nds, @discount, @image)";

            await database.ExecuteQuery(sql, data);
        }

        public async Task DeletePriceList(string guid)
        {
            await database.ExecuteQuery("DELETE FROM etk_app_price_list_template WHERE id = @guid", new { guid });
        }
    }
}
