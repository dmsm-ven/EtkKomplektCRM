using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IPriceListTemplateAdditionalTabsStorage
    {
        //quantity map
        Task AddQuantityMapRecord(string guid, string newQuantityMapRecordWord, int newQuantityMapRecordValue);
        Task RemoveQuantityMapRecord(string guid, string word);

        //name map
        Task AddManufacturerMapRecord(string guid, string newManufacturerMapRecordWord, int manufacturer_id);
        Task RemoveManufacturerMapRecord(string guid, string word);

        //white/black list
        Task RemoveSkipManufacturerRecord(string guid, int manufacturer_id, string listType);
        Task AddSkipManufacturerRecord(string guid, int manufacturer_id, string newSkipManufacturerListType);

        //discount map
        Task RemoveDiscountMapRecord(string guid, int manufacturer_id);
        Task AddDiscountMapRecord(string guid, int manufacturer_id, decimal discount);

        //purchase discounts
        Task AddPurchaseDiscountMapRecord(string guid, int manufacturer_id, decimal newDiscountMapValue);
        Task RemovePurchaseDiscountMapRecord(string guid, int manufacturer_id);
    }

    public class PriceListTemplateAdditionalTabsStorage : IPriceListTemplateAdditionalTabsStorage
    {
        private readonly IDatabaseAccess database;

        public PriceListTemplateAdditionalTabsStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task AddQuantityMapRecord(string guid, string newQuantityMapRecordWord, int newQuantityMapRecordValue)
        {
            string sql = @"INSERT INTO etk_app_price_list_template_quantity_map 
                            (price_list_guid, text, quantity) VALUES
                            (@guid, @text, @quantity)
                          ON DUPLICATE KEY UPDATE
                            price_list_guid = @guid,                            
                            text = @text, 
                            quantity = @quantity";

            await database.ExecuteQuery(sql, new { guid, text = newQuantityMapRecordWord, quantity = newQuantityMapRecordValue });
        }

        public async Task RemoveQuantityMapRecord(string guid, string word)
        {
            await database.ExecuteQuery(
                "DELETE FROM etk_app_price_list_template_quantity_map WHERE price_list_guid = @guid AND text = @word",
                new { guid, word });
        }

        public async Task AddManufacturerMapRecord(string guid, string text, int manufacturer_id)
        {
            string sql = @"INSERT INTO etk_app_price_list_template_manufacturer_map 
                            (price_list_guid, text, manufacturer_id) VALUES
                            (@guid, @text, @manufacturer_id)
                          ON DUPLICATE KEY UPDATE
                            price_list_guid = @guid,                            
                            text = @text, 
                            manufacturer_id = @manufacturer_id";

            await database.ExecuteQuery(sql, new { guid, text, manufacturer_id });
        }

        public async Task RemoveManufacturerMapRecord(string guid, string word)
        {
            await database.ExecuteQuery(
                "DELETE FROM etk_app_price_list_template_manufacturer_map WHERE price_list_guid = @guid AND text = @word",
                new { guid, word });
        }

        public async Task RemoveSkipManufacturerRecord(string guid, int manufacturer_id, string list_type)
        {
            string sql = @"DELETE FROM etk_app_price_list_template_manufacturer_list
                           WHERE price_list_guid = @guid AND manufacturer_id = @manufacturer_id AND list_type = @list_type";
            await database.ExecuteQuery(sql, new { guid, manufacturer_id, list_type });
        }

        public async Task AddSkipManufacturerRecord(string guid, int manufacturer_id, string list_type)
        {
            string sql = @"INSERT INTO etk_app_price_list_template_manufacturer_list
                            (price_list_guid, manufacturer_id, list_type) VALUES
                            (@guid, @manufacturer_id, @list_type)
                          ON DUPLICATE KEY UPDATE
                            price_list_guid = @guid,                            
                            manufacturer_id = @manufacturer_id, 
                            list_type = @list_type";

            await database.ExecuteQuery(sql, new { guid, manufacturer_id, list_type });
        }

        public async Task RemoveDiscountMapRecord(string guid, int manufacturer_id)
        {
            string sql = @"DELETE FROM etk_app_price_list_template_discount_map
                           WHERE price_list_guid = @guid AND manufacturer_id = @manufacturer_id";
            await database.ExecuteQuery(sql, new { guid, manufacturer_id });
        }

        public async Task AddDiscountMapRecord(string guid, int manufacturer_id, decimal discount)
        {
            string sql = @"INSERT INTO etk_app_price_list_template_discount_map
                            (price_list_guid, manufacturer_id, discount) VALUES
                            (@guid, @manufacturer_id, @discount)
                          ON DUPLICATE KEY UPDATE
                            price_list_guid = @guid,                            
                            manufacturer_id = @manufacturer_id, 
                            discount = @discount";

            await database.ExecuteQuery(sql, new { guid, manufacturer_id, discount });
        }

        public async Task AddPurchaseDiscountMapRecord(string guid, int manufacturer_id, decimal discount)
        {
            string sql = @"INSERT INTO etk_app_price_list_template_purchase_discount
                            (template_guid, manufacturer_id, discount) VALUES
                            (@guid, @manufacturer_id, @discount)
                          ON DUPLICATE KEY UPDATE
                            template_guid = @guid,                            
                            manufacturer_id = @manufacturer_id, 
                            discount = @discount";

            await database.ExecuteQuery(sql, new { guid, manufacturer_id, discount });
        }

        public async Task RemovePurchaseDiscountMapRecord(string guid, int manufacturer_id)
        {
            string sql = @"DELETE FROM etk_app_price_list_template_purchase_discount
                           WHERE template_guid = @guid AND manufacturer_id = @manufacturer_id";
            await database.ExecuteQuery(sql, new { guid, manufacturer_id });
        }
    }
}
