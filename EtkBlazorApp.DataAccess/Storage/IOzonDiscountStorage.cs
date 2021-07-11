using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IOzonSellerDiscountStorage
    {
        Task<List<OzonSellerManufacturerDiscountEntity>> GetManufacturerDiscounts();
        Task SaveDiscount(OzonSellerManufacturerDiscountEntity data);
    }

    public class OzonSellerDiscountStorage : IOzonSellerDiscountStorage
    {
        private readonly IDatabaseAccess database;

        public OzonSellerDiscountStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<OzonSellerManufacturerDiscountEntity>> GetManufacturerDiscounts()
        {
            string sql = @"SELECT m.manufacturer_id, 
                                IFNULL(o.discount, 0) as discount, 
                                IFNULL(o.enabled, 0) as enabled,  
                                m.name as manufacturer_name
                           FROM etk_app_ozon_seller_discount o
                           RIGHT JOIN oc_manufacturer m ON o.manufacturer_id = m.manufacturer_id";

            var discounts = await database.GetList<OzonSellerManufacturerDiscountEntity>(sql);
            return discounts;
        }

        public async Task SaveDiscount(OzonSellerManufacturerDiscountEntity template)
        {
            var sql = @"INSERT INTO etk_app_ozon_seller_discount
                           (manufacturer_id, enabled, discount)
                               VALUES
                               (@manufacturer_id, @enabled, @discount)
                               ON DUPLICATE KEY UPDATE
                               enabled = @enabled,
                               discount = @discount";

            await database.ExecuteQuery(sql, template);
        }

    }
}
