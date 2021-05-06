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
        Task CreatePriceList(PriceListTemplateEntity data);
        Task UpdatePriceList(PriceListTemplateEntity data);
        Task<List<PriceListTemplateEntity>> GetPriceListTemplates();
        Task<PriceListTemplateEntity> GetPriceListTemplateById(string guid);
        Task DeletePriceList(string guid);
        Task ChangePriceListTemplateDiscount(string guid, decimal discount);
        Task<List<PriceListTemplateRemoteUriMethodEntity>> GetPricelistTemplateRemoteLoadMethods();
        Task<List<PriceListTemplateContentTypeEntity>> GetPriceListTemplateContentTypes();
        Task<List<string>> GetPriceListTemplatGroupNames();      
    }

    public class PriceListTemplateStorage : IPriceListTemplateStorage
    {
        private readonly IDatabaseAccess database;

        public PriceListTemplateStorage(IDatabaseAccess database)
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
            string sql = @"SELECT t.*, 
                                    ct.name as content_type_name, 
                                    lm.name as remote_uri_method_name, 
                                    cred.login as credentials_login, cred.password as credentials_password,
                                    esc.subject as email_criteria_subject, 
                                    esc.sender as email_criteria_sender, 
                                    esc.file_name_pattern as email_criteria_file_name_pattern,
                                    esc.max_age_in_days as email_criteria_max_age_in_days
                          FROM etk_app_price_list_template t
                          LEFT JOIN etk_app_price_list_template_content_type ct ON t.content_type_id = ct.id
                          LEFT JOIN etk_app_price_list_template_load_method lm ON t.remote_uri_method_id = lm.id 
                          LEFT JOIN etk_app_price_list_template_email_search_criteria esc ON t.id = esc.template_guid 
                          LEFT JOIN etk_app_price_list_template_credentials cred ON t.id = cred.template_guid 
                          WHERE t.id = @guid LIMIT 1";

            var templateInfo = await database.GetFirstOrDefault<PriceListTemplateEntity, dynamic>(sql, new { guid });
            return templateInfo;
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

            if (data.credentials_login != null && data.credentials_password != null)
            {
                await InsertOrUpdatePriceListCredentials(data);
            }
            if (data.email_criteria_sender != null)
            {              
                await InsertOrUpdatePriceListEmailSearchCriteria(data);
            }
        }

        private async Task InsertOrUpdatePriceListEmailSearchCriteria(PriceListTemplateEntity data)
        {
            string sql = @"INSERT INTO etk_app_price_list_template_email_search_criteria 
                            (template_guid, subject, sender, file_name_pattern, max_age_in_days) VALUES
                            (@id, @email_criteria_subject, @email_criteria_sender, @email_criteria_file_name_pattern, @email_criteria_max_age_in_days)
                          ON DUPLICATE KEY UPDATE
                          subject = @email_criteria_subject, 
                              sender = @email_criteria_sender, 
                              file_name_pattern = @email_criteria_file_name_pattern,
                              max_age_in_days = @email_criteria_max_age_in_days";

            await database.ExecuteQuery(sql, data);
        }
        
        private async Task InsertOrUpdatePriceListCredentials(PriceListTemplateEntity data)
        {
            string sql = @"INSERT INTO etk_app_price_list_template_credentials (template_guid, login, password) VALUES
                            (@id, @credentials_login, @credentials_password)
                          ON DUPLICATE KEY UPDATE
                          login = @credentials_login, password = @credentials_password";

            await database.ExecuteQuery(sql, data);
        }

        public async Task CreatePriceList(PriceListTemplateEntity data)
        {          
            string sql = @"INSERT INTO etk_app_price_list_template
                        (id, title, description, group_name, content_type_id, remote_uri, remote_uri_method_id, nds, discount, image) VALUES
                        (@id, @title, @description, @group_name, @content_type_id, @remote_uri, @remote_uri_method_id, @nds, @discount, @image)";

            await database.ExecuteQuery(sql, data);

            if (data.credentials_login != null && data.credentials_password != null)
            {
                await InsertOrUpdatePriceListEmailSearchCriteria(data);
            }
            if (data.email_criteria_sender != null)
            {
                await InsertOrUpdatePriceListCredentials(data);
            }

        }

        public async Task DeletePriceList(string guid)
        {
            await database.ExecuteQuery("DELETE FROM etk_app_price_list_template WHERE id = @guid", new { guid });
            await database.ExecuteQuery("DELETE FROM etk_app_price_list_email_search_criteria WHERE template_id = @guid", new { guid });
            await database.ExecuteQuery("DELETE FROM etk_app_price_list_credentials WHERE template_id = @guid", new { guid });
        }
    }
}
