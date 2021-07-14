using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IProductDiscountStorage
    {
        Task<List<ProductSpecialEntity>> GetProductsWithDiscount();
        Task AddDiscountForProduct(ProductSpecialEntity discountData);
        Task RemoveProductDiscount(int product_id);

        Task<List<DiscountToManufacturerEntity>> GetManufacturersWithDiscount();
        Task AddDiscountForManufacturer(DiscountToManufacturerEntity discountData);
        Task RemoveManufacturerDiscount(int manufacturer_id);

        Task<List<DiscountToCategoryEntity>> GetCategoriesWithDiscount();
        Task AddDiscountForCategory(DiscountToCategoryEntity discountData);
        Task RemoveCategoryDiscount(int category_id);

        Task<List<DiscountToStockEntity>> GetStocksWithDiscount();
        Task AddDiscountForStock(DiscountToStockEntity discountData, int minQuantity);
        Task RemoveStockDiscount(int stock_id);
    }

    public class ProductDiscountStorage : IProductDiscountStorage
    {
        readonly int SALE_CATEGORY_ID = 60396;
        readonly int STOCK_LEVEL_PRIORITY = 4;
        readonly int MANUFACTURER_LEVEL_PRIORITY = 3;
        readonly int CATEGORY_LEVEL_PRIORITY = 2;
        readonly int PRODUCT_LEVEL_PRIORITY = 1;
        readonly int DEFAULT_CUSTOMER_GROUP_ID = 1;

        private readonly IDatabaseAccess database;

        public ProductDiscountStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        private async Task RefreshMainDiscountCategoryProducts()
        {
            string sql = @"INSERT IGNORE INTO oc_product_to_category (product_id, category_id, main_category)
                           SELECT product_id, @SALE_CATEGORY_ID, 0
                           FROM oc_product_special
                           WHERE DATE(date_end) >= DATE(NOW())";

            await database.ExecuteQuery<dynamic>(sql, new { SALE_CATEGORY_ID });

            sql = @"DELETE FROM oc_product_to_category 
                    WHERE category_id = @SALE_CATEGORY_ID AND 
                    product_id NOT IN (
                        SELECT product_id FROM oc_product_special WHERE DATE(date_end) >= DATE(NOW())
                        );";

            await database.ExecuteQuery<dynamic>(sql, new { SALE_CATEGORY_ID });
        }

        #region Stock

        public async Task<List<DiscountToStockEntity>> GetStocksWithDiscount()
        {
            var sql = @"SELECT s.*, sp.name as stock_name
                        FROM etk_app_discount_to_stock s
                        JOIN oc_stock_partner sp ON sp.stock_partner_id = s.stock_id";

            var discounts = await database.GetList<DiscountToStockEntity>(sql);

            return discounts;
        }

        public async Task AddDiscountForStock(DiscountToStockEntity discountData, int minQuantity = 1)
        {
            var sql = @"INSERT INTO etk_app_discount_to_stock
                           (stock_id, discount, date_start, date_end)
                           VALUES
                           (@stock_id, @discount, @date_start, @date_end)
                           ON DUPLICATE KEY UPDATE
                           discount = @discount,
                           date_start = @date_start,
                           date_end = @date_end";

            await database.ExecuteQuery(sql, discountData);
            await RemoveProductSpecialOnStock(discountData.stock_id);
            await AddProductSpecialForStock(discountData, minQuantity);
        }

        public async Task RemoveStockDiscount(int stock_id)
        {
            string sql = @"DELETE FROM etk_app_discount_to_stock
                           WHERE stock_id = @stock_id";

            await database.ExecuteQuery<dynamic>(sql, new { stock_id });
            await RemoveProductSpecialOnStock(stock_id);
        }

        private async Task AddProductSpecialForStock(DiscountToStockEntity discountData, int minQuantity)
        {
            string rubStatement = $"ROUND(p.price / (100 + {discountData.discount}) * 100, 0)";
            string curStatement = $"ROUND(p.base_price / (100 + {discountData.discount}) * 100, 2)";

            string sql = @$"INSERT INTO oc_product_special 
                          (product_id, customer_group_id, priority, price, base_price, date_start, date_end)
                           SELECT p.product_id, 
                                  {DEFAULT_CUSTOMER_GROUP_ID}, 
                                  {STOCK_LEVEL_PRIORITY},
                                  IF(p.base_currency_code = 'RUB', {rubStatement}, 0), 
                                  IF(p.base_currency_code = 'RUB', 0, {curStatement}), 
                                  DATE(@date_start), DATE(@date_end)
                           FROM oc_product p
                           JOIN oc_product_to_stock pts ON (p.product_id = pts.product_id AND pts.stock_partner_id = @stock_id)
                           WHERE (p.status = 1) AND (pts.quantity >= {minQuantity}) AND (p.price > 0 OR p.base_price > 0)";

            await database.ExecuteQuery(sql, discountData);
            await RefreshMainDiscountCategoryProducts();
        }

        private async Task RemoveProductSpecialOnStock(int stock_id)
        {
            string sql = $@"DELETE FROM oc_product_special
                           WHERE priority = @STOCK_LEVEL_PRIORITY AND 
                                 product_id IN (SELECT product_id FROM oc_product_to_stock WHERE stock_partner_id = @stock_id)";

            await database.ExecuteQuery<dynamic>(sql, new { STOCK_LEVEL_PRIORITY, stock_id });
            await RefreshMainDiscountCategoryProducts();
        }

        #endregion

        #region Manufacturer
        public async Task AddDiscountForManufacturer(DiscountToManufacturerEntity discountData)
        {
            var sql = @"INSERT INTO etk_app_discount_to_manufacturer
                       (manufacturer_id, discount, date_start, date_end)
                       VALUES
                       (@manufacturer_id, @discount, @date_start, @date_end)
                       ON DUPLICATE KEY UPDATE
                       discount = @discount,
                       date_start = @date_start,
                       date_end = @date_end";

            await database.ExecuteQuery(sql, discountData);

            await RemoveProductSpecialOnManufacturer(discountData.manufacturer_id);
            await AddProductSpecialForManufacturer(discountData);
        }

        public async Task RemoveManufacturerDiscount(int manufacturer_id)
        {
            string sql = @"DELETE FROM etk_app_discount_to_manufacturer 
                           WHERE manufacturer_id = @manufacturer_id";

            await database.ExecuteQuery<dynamic>(sql, new { manufacturer_id });
            await RemoveProductSpecialOnManufacturer(manufacturer_id);
        }

        public async Task<List<DiscountToManufacturerEntity>> GetManufacturersWithDiscount()
        {
            var sql = @"SELECT d.*, m.name as manufacturer_name
                        FROM etk_app_discount_to_manufacturer d
                        JOIN oc_manufacturer m ON d.manufacturer_id = m.manufacturer_id";

            var discounts = await database.GetList<DiscountToManufacturerEntity>(sql);

            return discounts;
        }

        private async Task RemoveProductSpecialOnManufacturer(int manufacturer_id)
        {
            string sql = $@"DELETE FROM oc_product_special
                          WHERE oc_product_special.priority = @MANUFACTURER_LEVEL_PRIORITY AND product_id IN 
                            (SELECT product_id FROM oc_product WHERE manufacturer_id = @manufacturer_id)";

            await database.ExecuteQuery<dynamic>(sql, new { MANUFACTURER_LEVEL_PRIORITY, manufacturer_id });
            await RefreshMainDiscountCategoryProducts();
        }

        private async Task AddProductSpecialForManufacturer(DiscountToManufacturerEntity discountData)
        {
            string rubStatement = $"ROUND(p.price / (100 + {discountData.discount}) * 100, 0)";
            string curStatement = $"ROUND(p.base_price / (100 + {discountData.discount}) * 100, 2)";

            string sql = @$"INSERT INTO oc_product_special (product_id, customer_group_id, priority, price, base_price, date_start, date_end)
                           SELECT product_id, 
                                  {DEFAULT_CUSTOMER_GROUP_ID}, 
                                  {MANUFACTURER_LEVEL_PRIORITY}, 
                                  IF(p.base_currency_code = 'RUB', {rubStatement}, 0), 
                                  IF(p.base_currency_code = 'RUB', 0, {curStatement}), 
                                  DATE(@date_start), DATE(@date_end)
                           FROM oc_product p
                           JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                           WHERE p.status = 1 AND m.manufacturer_id = @manufacturer_id AND (p.price > 0 OR p.base_price > 0)";

            await database.ExecuteQuery(sql, discountData);
            await RefreshMainDiscountCategoryProducts();
        }
        #endregion

        #region Category
        public async Task<List<DiscountToCategoryEntity>> GetCategoriesWithDiscount()
        {
            var sql = @"SELECT c.*, d.name as category_name
                        FROM etk_app_discount_to_category c
                        JOIN oc_category_description d ON c.category_id = d.category_id";

            var discounts = await database.GetList<DiscountToCategoryEntity>(sql);

            return discounts;
        }

        public async Task RemoveCategoryDiscount(int category_id)
        {
            string sql = @"DELETE FROM etk_app_discount_to_category
                           WHERE category_id = @category_id";

            await database.ExecuteQuery<dynamic>(sql, new { category_id });
            await RemoveProductSpecialOnCategory(category_id);
        }

        private async Task AddProductSpecialForCategory(DiscountToCategoryEntity discountData)
        {
            string rubStatement = $"ROUND(p.price / (100 + {discountData.discount}) * 100, 0)";
            string curStatement = $"ROUND(p.base_price / (100 + {discountData.discount}) * 100, 2)";

            string sql = @$"INSERT INTO oc_product_special 
                          (product_id, customer_group_id, priority, price, base_price, date_start, date_end)
                           SELECT p.product_id, 
                                  {DEFAULT_CUSTOMER_GROUP_ID}, 
                                  {CATEGORY_LEVEL_PRIORITY},
                                  IF(p.base_currency_code = 'RUB', {rubStatement}, 0), 
                                  IF(p.base_currency_code = 'RUB', 0, {curStatement}), 
                                  DATE(@date_start), DATE(@date_end)
                           FROM oc_product p
                           JOIN oc_product_to_category ptc ON p.product_id = ptc.product_id
                           WHERE p.status = 1 AND ptc.category_id = @category_id AND (p.price > 0 OR p.base_price > 0)";

            await database.ExecuteQuery(sql, discountData);
            await RefreshMainDiscountCategoryProducts();
        }

        private async Task RemoveProductSpecialOnCategory(int category_id)
        {
            string sql = @"DELETE FROM oc_product_special
                          WHERE oc_product_special.priority = @CATEGORY_LEVEL_PRIORITY AND product_id IN 
                            (SELECT product_id FROM oc_product WHERE product_id IN 
                                (SELECT DISTINCT product_id FROM oc_product_to_category WHERE category_id = @category_id))";

            await database.ExecuteQuery<dynamic>(sql, new { CATEGORY_LEVEL_PRIORITY, category_id });
            await RefreshMainDiscountCategoryProducts();
        }

        public async Task AddDiscountForCategory(DiscountToCategoryEntity discountData)
        {
            var sql = @"INSERT INTO etk_app_discount_to_category
                       (category_id, discount, date_start, date_end)
                       VALUES
                       (@category_id, @discount, @date_start, @date_end)
                       ON DUPLICATE KEY UPDATE
                       discount = @discount,
                       date_start = @date_start,
                       date_end = @date_end";

            await database.ExecuteQuery(sql, discountData);

            await RemoveProductSpecialOnCategory(discountData.category_id);
            await AddProductSpecialForCategory(discountData);
        }
        #endregion

        #region Product

        public async Task<List<ProductSpecialEntity>> GetProductsWithDiscount()
        {
            var sql = $@"SELECT p.product_id, d.name as name, m.name as manufacturer, p.base_currency_code,
                                p.price as RegularPriceInRub, 
                                sp.price as NewPriceInRub, 
                                p.base_price as RegularPriceInCurrency, 
                                sp.base_price as NewPriceInCurrency,
                                sp.date_start, sp.date_end

                        FROM oc_product p
                        JOIN oc_product_special sp ON p.product_id = sp.product_id
                        JOIN oc_product_description d ON p.product_id = d.product_id
                        JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                        WHERE p.status = 1 AND priority <= {PRODUCT_LEVEL_PRIORITY} AND (NOW() BETWEEN sp.date_start AND sp.date_end)";

            var products = await database.GetList<ProductSpecialEntity>(sql);

            return products;
        }

        public async Task AddDiscountForProduct(ProductSpecialEntity discountData)
        {
            string priceRub = discountData.base_currency_code == "RUB" ? discountData.NewPriceInRub.ToString("F0") : "0";
            string priceCur = discountData.base_currency_code == "RUB" ? "0" : discountData.NewPriceInCurrency.ToString("F2").Replace(",", ".");
            string sql = @$"INSERT INTO oc_product_special 
                          (product_id, customer_group_id, priority, price, base_price, date_start, date_end) VALUES
                          (@product_id, 
                            {DEFAULT_CUSTOMER_GROUP_ID}, 
                            {PRODUCT_LEVEL_PRIORITY},
                            {priceRub}, 
                            {priceCur}, 
                            DATE(@date_start), DATE(@date_end))";

            await database.ExecuteQuery(sql, discountData);
            await RefreshMainDiscountCategoryProducts();
        }

        public async Task RemoveProductDiscount(int product_id)
        {
            string sql = $@"DELETE FROM oc_product_special
                           WHERE product_id = @product_id AND priority = {PRODUCT_LEVEL_PRIORITY}";

            await database.ExecuteQuery<dynamic>(sql, new { product_id });

            sql = @"DELETE FROM oc_product_to_category 
                    WHERE category_id = @SALE_CATEGORY_ID AND product_id = @product_id";

            await database.ExecuteQuery<dynamic>(sql, new { SALE_CATEGORY_ID, product_id });
        }

        #endregion
   }
}
