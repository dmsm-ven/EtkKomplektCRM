using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IPartnersInformationService
    {
        Task<List<PartnerEntity>> GetAllPartners();        
        Task AddOrUpdatePartner(PartnerEntity partner);
        Task DeletePartner(string guid);
        Task<DateTime?> GetPartnerLastAccessDateTime(string guid);
        Task<List<DateTime>> GetPartnerRequestHistory(string guid, int limit);

        Task AddManufacturerToPartner(string partner_id, int manufacturer_id, decimal? discount);
        Task RemoveManufacturerFromPartner(string partner_id, int manufacturer_id);
        Task<List<PartnerManufacturerDiscountEntity>> GetPartnerManufacturers(string partner_id);
    }

    public class PartnersInformationService : IPartnersInformationService
    {
        private readonly IDatabaseAccess database;

        public PartnersInformationService(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<PartnerEntity>> GetAllPartners()
        {
            string sql = @"SELECT p.*, 
                                (SELECT date_time FROM etk_app_partner_request_history WHERE partner_id = p.id ORDER BY date_time DESC LIMIT 1) as price_list_last_access
                           FROM etk_app_partner p";

            var list = await database.GetList<PartnerEntity>(sql);

            return list;
        }

        public async Task AddOrUpdatePartner(PartnerEntity partner)
        {
            string sql = @"INSERT INTO etk_app_partner 
                (id, name, website, email, phone_number, address, description, priority, discount, contact_person, created, updated, price_list_password) VALUES 
                (@id, @name, @website, @email, @phone_number, @address, @description, @priority, @discount, @contact_person, @created, @updated, @price_list_password)
                ON DUPLICATE KEY UPDATE 
                            name = @name,
                            website = @website,
                            email = @email,
                            phone_number = @phone_number,
                            address = @address,
                            description = @description,
                            priority = @priority,
                            discount = @discount,
                            contact_person = @contact_person,
                            updated = @updated";

            await database.ExecuteQuery(sql, partner);
        }

        public async Task DeletePartner(string partner_id)
        {
            await database.ExecuteQuery("DELETE FROM etk_app_partner_checked_brand WHERE partner_id = @partner_id", new { partner_id });
            await database.ExecuteQuery("DELETE FROM etk_app_partner WHERE id = @partner_id", new { partner_id });
        }

        public async Task AddManufacturerToPartner(string partner_id, int manufacturer_id, decimal? discount)
        {
            string sql = @"INSERT INTO etk_app_partner_checked_brand (partner_id, manufacturer_id, discount) 
                          VALUES (@partner_id, @manufacturer_id, @discount)
                          ON DUPLICATE KEY UPDATE discount = @discount";

            await database.ExecuteQuery(sql, new { partner_id, manufacturer_id, discount });
            await database.ExecuteQuery("UPDATE etk_app_partner SET updated = NOW() WHERE id = @partner_id", new { partner_id });
        }

        public async Task RemoveManufacturerFromPartner(string partner_id, int manufacturer_id)
        {
            string sql = "DELETE FROM etk_app_partner_checked_brand WHERE partner_id = @partner_id AND manufacturer_id = @manufacturer_id";
            await database.ExecuteQuery(sql, new { partner_id, manufacturer_id });
            await database.ExecuteQuery("UPDATE etk_app_partner SET updated = NOW() WHERE id = @partner_id", new { partner_id });
        }

        public async Task<List<PartnerManufacturerDiscountEntity>> GetPartnerManufacturers(string partner_id)
        {
            string sql = @"SELECT p.*, m.name
                           FROM etk_app_partner_checked_brand p                        
                           JOIN oc_manufacturer m ON (p.manufacturer_id = m.manufacturer_id)
                           WHERE p.partner_id = @partner_id";

            var data = await database.GetList<PartnerManufacturerDiscountEntity, dynamic>(sql, new { partner_id });

            return data;
        }

        public async Task<DateTime?> GetPartnerLastAccessDateTime(string partner_id)
        {
            string sql = "SELECT date_time FROM etk_app_partner_request_history WHERE partner_id = @partner_id ORDER BY date_time DESC LIMIT 1";
            var dt = await database.GetScalar<DateTime?, dynamic>(sql, new { partner_id });
            return dt;
        }

        public async Task<List<DateTime>> GetPartnerRequestHistory(string partner_id, int limit)
        {
            string sql = "SELECT date_time FROM etk_app_partner_request_history WHERE partner_id = @partner_id ORDER BY date_time DESC LIMIT @limit";
            var dt = await database.GetList<DateTime, dynamic>(sql, new { partner_id, limit });
            return dt;
        }
    }
}
