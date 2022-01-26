using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.DataAccess
{
    public interface IProductStorage
    {
        Task<List<ProductEntity>> GetLastAddedProducts(int count);
        Task<List<ProductEntity>> GetBestsellersByQuantity(int count, int maxOrderOldInDays);
        Task<List<ProductEntity>> GetBestsellersBySum(int count, int maxOrderOldInDays);
        Task<List<ProductEntity>> GetProductQuantityInAdditionalStock(int stockId);
        Task<List<ProductEntity>> ReadProducts(IEnumerable<int> allowedManufacturers = null);
        Task<List<ProductEntity>> ReadProducts(int manufacturer_id);
        Task<List<ProductEntity>> SearchProductsByName(string searchText);

        Task<ProductEntity> GetProductByKeyword(string keyword);
        Task<ProductEntity> GetProductByModel(string model);
        Task<ProductEntity> GetProductBySku(string sku);
        Task<ProductEntity> GetProductById(int id);

        Task<List<StockStatusEntity>> GetStockStatuses();

        Task<List<ProductEntity>> GetFixedProducts();
        Task RemoveFixedProduct(int id);
        Task AddFixedProduct(ProductEntity product);
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

        public Task<ProductEntity> GetProductById(int product_id)
        {
            return GetSingleProductOrNullByField(nameof(product_id), product_id.ToString());
        }

        public async Task<ProductEntity> GetProductByKeyword(string keyword)
        {
            string sql = @"SELECT p.*, d.name as name, s.name as stock_status, url.keyword as keyword, pr.replacement_id, m.name as manufacturer
                           FROM oc_product p
                           JOIN oc_url_alias url ON url.query = CONCAT('product_id=', p.product_id)
                           LEFT JOIN oc_product_replacement pr ON p.product_id = pr.product_id
                           JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
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
                .AppendLine("GROUP BY p.product_id")
                .AppendLine($"ORDER BY SUM(op.{columnOrder}) DESC")
                .AppendLine("LIMIT @Limit");

            string sql = sb.ToString();

            var products = await database.GetList<ProductEntity, dynamic>(sql, new { Limit = count, maxOrderOldInDays = -maxOrderOldInDays });

            return products;
        }

        private async Task<ProductEntity> GetSingleProductOrNullByField(string fieldName, string fieldValue)
        {
            string sql = @$"SELECT p.*, url.keyword as keyword, m.name as manufacturer, d.name as name
                         FROM oc_product p
                         JOIN oc_product_description d ON p.product_id = d.product_id
                         LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                         LEFT JOIN oc_url_alias url ON url.query = CONCAT('product_id=', p.product_id)
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

        public async Task<List<ProductEntity>> SearchProductsByName(string searchText)
        {
            var sql = @"SELECT p.*, d.name as name, m.name as manufacturer
                        FROM oc_product p
                        JOIN oc_product_description d ON p.product_id = d.product_id
                        JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                        WHERE p.status = 1 AND d.name LIKE @pattern
                        ORDER BY d.name
                        LIMIT 10";

            var findedProducts = await database.GetList<ProductEntity, dynamic>(sql, new { pattern = $"%{searchText}%" });
            findedProducts.ForEach(p => p.name = HttpUtility.HtmlDecode(p.name));

            return findedProducts;
        }

        public async Task<List<ProductEntity>> GetProductQuantityInAdditionalStock(int stockParnerId)
        {
            string sql = "SELECT * FROM oc_product_to_stock WHERE stock_partner_id = @stockParnerId";

            var data = await database.GetList<ProductEntity, dynamic>(sql, new { stockParnerId });

            return data;
        }

        public async Task<List<ProductEntity>> GetFixedProducts()
        {
            string sql = @"SELECT fp.*, pd.name 
                          FROM etk_app_fixed_product fp LEFT JOIN oc_product_description pd ON fp.product_id = pd.product_id";

            var list = await database.GetList<ProductEntity>(sql);

            return list;
        }

        public async Task RemoveFixedProduct(int id)
        {
            string sql = @"DELETE FROM etk_app_fixed_product WHERE product_id = @id";
            await database.ExecuteQuery<dynamic>(sql, new { id });

        }

        public async Task AddFixedProduct(ProductEntity product)
        {
            string sql = @"INSERT INTO etk_app_fixed_product (product_id) VALUES (@product_id)";
            await database.ExecuteQuery(sql, product);
        }
    }
}
