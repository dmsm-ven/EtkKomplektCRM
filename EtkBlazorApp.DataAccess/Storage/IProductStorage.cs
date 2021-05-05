using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IProductStorage
    {
        Task<List<ProductEntity>> GetLastAddedProducts(int count);
        Task<List<ProductEntity>> GetProductsWithMaxDiscount(int count);
        Task<List<ProductEntity>> GetBestsellersByQuantity(int count, int maxOrderOldInDays);
        Task<List<ProductEntity>> GetBestsellersBySum(int count, int maxOrderOldInDays);

        Task<ProductEntity> GetProductByKeyword(string keyword);
        Task<ProductEntity> GetProductByModel(string model);
        Task<ProductEntity> GetProductBySku(string sku);

        Task<List<ProductEntity>> ReadProducts(IEnumerable<int> allowedManufacturers = null);
        Task<List<ProductEntity>> ReadProducts(int manufacturer_id);      
        Task<List<StockStatusEntity>> GetStockStatuses();
    }

    public class ProductStorage : IProductStorage
    {
        private readonly IDatabaseAccess database;

        public ProductStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<ProductEntity>> GetLastAddedProducts(int count)
        {
                var sql = @"SELECT p.*, d.name as name, m.name as manufacturer
                            FROM oc_product p
                            JOIN oc_product_description d ON p.product_id = d.product_id
                            JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                            WHERE p.status = 1
                            ORDER BY p.product_id DESC
                            LIMIT @Limit";

            var products = await database.GetList<ProductEntity, dynamic>(sql, new { Limit = count });

            return products;
        }

        public async Task<List<ProductEntity>> GetProductsWithMaxDiscount(int count)
        {          
            var sql = @"SELECT p.*, d.name as name, m.name as manufacturer, sp.price as discount_price
                        FROM oc_product p
                        JOIN oc_product_special sp ON p.product_id = sp.product_id
                        JOIN oc_product_description d ON p.product_id = d.product_id
                        JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                        WHERE p.status = 1 AND (NOW() BETWEEN sp.date_start AND sp.date_end)
                        ORDER BY Round(sp.price / p.price, 4)
                        LIMIT @Limit";

            var products = await database.GetList<ProductEntity, dynamic>(sql, new { Limit = count });

            return products;
        }

        public Task<List<ProductEntity>> GetBestsellersBySum(int count, int maxOrderOldInDays)
        {
            return GetBestsellersByField(count, maxOrderOldInDays, nameof(OrderDetailsEntity.total));
        }

        public Task<List<ProductEntity>> GetBestsellersByQuantity(int count, int maxOrderOldInDays)
        {
            return GetBestsellersByField(count, maxOrderOldInDays, nameof(OrderDetailsEntity.quantity));
        }

        public Task<ProductEntity> GetProductByModel(string model)
        {
            return GetSingleProductOrNullByField(nameof(model), model);
        }

        public Task<ProductEntity> GetProductBySku(string sku)
        {
            return GetSingleProductOrNullByField(nameof(sku), sku);
        }

        public async Task<ProductEntity> GetProductByKeyword(string keyword)
        {
            string sql = @"SELECT p.*, d.name as name, s.name as stock_status, url.keyword as keyword
                           FROM oc_product p
                           JOIN oc_url_alias url ON url.query = CONCAT('product_id=', p.product_id)
                           JOIN oc_product_description d ON p.product_id = d.product_id
                           JOIN oc_stock_status s ON s.stock_status_id = p.stock_status_id
                           WHERE url.keyword = @keyword
                           LIMIT 1";

            var product = await database.GetFirstOrDefault<ProductEntity, dynamic>(sql, new { keyword });

            return product;
        }
      
        public async Task<List<StockStatusEntity>> GetStockStatuses()
        {
            var data = await database.GetList<StockStatusEntity>("SELECT * FROM oc_stock_status");
            return data;
        }

        public async Task<List<ProductEntity>> ReadProducts(IEnumerable<int> allowedManufacturers = null)
        {
            if (allowedManufacturers != null && allowedManufacturers.Count() == 0)
            {
                return new List<ProductEntity>();
            }

            var sb = new StringBuilder()
                    .AppendLine("SELECT p.*, d.name as name, m.name as manufacturer, url.keyword as url")
                    .AppendLine("FROM oc_product p")
                    .AppendLine("LEFT JOIN oc_product_description d ON p.product_id = d.product_id")
                    .AppendLine("LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                    .AppendLine("LEFT JOIN oc_url_alias url ON CONCAT('product_id=', p.product_id) = url.query")
                    .AppendLine("WHERE p.status = 1 AND d.main_product = '0'");

            if (allowedManufacturers != null && allowedManufacturers.Any())
            {
                string allowedIdArray = string.Join(",", allowedManufacturers);
                sb.AppendLine($"AND p.manufacturer_id IN ({allowedIdArray})");
            }

            sb.Append("ORDER BY m.name, d.name");

            string sql = sb.ToString().Trim();

            var products = await database.GetList<ProductEntity>(sql);

            return products;

        }

        public Task<List<ProductEntity>> ReadProducts(int manufacturer_id)
        {
            return ReadProducts(new List<int>(new[] { manufacturer_id }));
        }        

        private async Task<List<ProductEntity>> GetBestsellersByField(int count, int maxOrderOldInDays, string columnOrder)
        {
            var sb = new StringBuilder()
                .AppendLine("SELECT p.*, d.name as name, m.name as manufacturer")
                .AppendLine("FROM oc_product p")
                .AppendLine("LEFT JOIN oc_order_product op ON op.product_id = p.product_id")
                .AppendLine("LEFT JOIN oc_order o ON o.order_id = op.order_id")
                .AppendLine("JOIN oc_product_description d ON p.product_id = d.product_id")
                .AppendLine("JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                .AppendLine("WHERE DATE(o.date_added) > DATE_ADD(NOW(), INTERVAL @maxOrderOldInDays DAY)")
                .AppendLine("GROUP BY op.product_id")
                .AppendLine($"ORDER BY SUM(op.{columnOrder}) DESC")
                .AppendLine("LIMIT @Limit");

            string sql = sb.ToString();

            var products = await database.GetList<ProductEntity, dynamic>(sql, new { Limit = count, maxOrderOldInDays = -maxOrderOldInDays });

            return products;
        }

        private async Task<ProductEntity> GetSingleProductOrNullByField(string fieldName, string fieldValue)
        {
            string sql = @$"SELECT p.*, url.keyword as keyword
                         FROM oc_product p
                         JOIN oc_url_alias url ON url.query = CONCAT('product_id=', p.product_id)
                         WHERE p.{fieldName} = @fieldValue";

            try
            {
                var products = await database.GetList<ProductEntity, dynamic>(sql, new { fieldValue });
                return products.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
