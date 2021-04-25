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
        Task<ProductEntity> GetProductByKeyword(string keyword);
        Task<ProductEntity> GetProductByModel(string model);
        Task<ProductEntity> GetProductBySku(string sku);
       
        Task UpdateProductsPrice(List<ProductUpdateData> data);
        Task UpdateProductsStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate);
        Task UpdateProductsStockPartner(List<ProductUpdateData> data);

        Task<List<ProductEntity>> ReadProducts(IEnumerable<int> allowedManufacturers = null);
        Task<List<ProductEntity>> ReadProducts(int manufacturer_id);
        Task UpdateDirectProduct(ProductEntity product);
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
            int MAX_LIMIT_PER_REQUEST = 100;

            if (count > MAX_LIMIT_PER_REQUEST)
            {
                throw new ArgumentOutOfRangeException($"Превышено максимальное количество запрашиваемых товаров ({MAX_LIMIT_PER_REQUEST})");
            }

            var sb = new StringBuilder()
                .AppendLine("SELECT p.*, d.name as name, m.name as manufacturer")
                .AppendLine("FROM oc_product p")
                .AppendLine("JOIN oc_product_description d ON p.product_id = d.product_id")
                .AppendLine("JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                .AppendLine("WHERE p.status = 1")
                .AppendLine("ORDER BY p.product_id DESC")
                .AppendLine("LIMIT @Limit");

            string sql = sb.ToString();

            var products = await database.LoadData<ProductEntity, dynamic>(sql, new { Limit = count });

            return products.ToList();
        }

        public async Task<ProductEntity> GetProductByModel(string model) => await GetSingleProductOrNullByField(nameof(model), model);

        public async Task<ProductEntity> GetProductBySku(string sku) => await GetSingleProductOrNullByField(nameof(sku), sku);

        private async Task<ProductEntity> GetSingleProductOrNullByField(string fieldName, string fieldValue)
        {
            string sql = "SELECT p.*, url.keyword as keyword " +
                         "FROM oc_product p " +
                         "JOIN oc_url_alias url ON url.query = CONCAT('product_id=', p.product_id) " +
                         $"WHERE p.{fieldName} = @fieldValue ";

            try
            {
                var product = (await database.LoadData<ProductEntity, dynamic>(sql, new { fieldValue })).SingleOrDefault();
                return product;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public async Task<ProductEntity> GetProductByKeyword(string keyword)
        {
            string sql = "SELECT p.product_id, d.name, p.sku, p.model, p.quantity, p.price, p.base_price, p.base_currency_code, p.date_modified, s.name as stock_status, url.keyword as keyword " +
                         "\nFROM oc_product p " +
                         "\nJOIN oc_url_alias url ON url.query = CONCAT('product_id=', p.product_id) " +
                         "\nLEFT JOIN oc_product_description d ON p.product_id = d.product_id " +
                         "\nLEFT JOIN oc_stock_status s ON s.stock_status_id = p.stock_status_id " +
                         "\nWHERE url.keyword = @keyword " +
                         "\nLIMIT 1";

            var product = (await database.LoadData<ProductEntity, dynamic>(sql, new { keyword })).FirstOrDefault();
            return product;
        }

        public async Task UpdateDirectProduct(ProductEntity product)
        {
            var sb = new StringBuilder()
                .AppendLine("UPDATE oc_product")
                .AppendLine("SET price = @price,")
                .AppendLine("base_price = @base_price,")
                .AppendLine("base_currency_code = @base_currency_code,")
                .AppendLine("quantity = @quantity,")
                .AppendLine("stock_status_id = (SELECT stock_status_id FROM oc_stock_status WHERE name = @stock_status),")
                .AppendLine("date_modified = NOW()")
                .AppendLine("WHERE product_id = @product_id");

            string sql = sb.ToString();
            await database.SaveData(sql, product);
        }

        public async Task<List<StockStatusEntity>> GetStockStatuses()
        {
            var data = await database.LoadData<StockStatusEntity, dynamic>("SELECT * FROM oc_stock_status", new { });
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

            var products = await database.LoadData<ProductEntity, dynamic>(sql, new { });

            return products;

        }

        public Task<List<ProductEntity>> ReadProducts(int manufacturer_id)
        {
            return ReadProducts(new List<int>(new[] { manufacturer_id }));
        }

        public async Task UpdateProductsPrice(List<ProductUpdateData> data)
        {
            List<int> discountProductIds = await GetDiscountProductIds();

            List<ProductUpdateData> source = data
                .Where(d => d.price.HasValue && !discountProductIds.Contains(d.product_id) && d.stock_partner == null)
                .ToList();

            Dictionary<string, List<int>> idsGroupedByCurrency = source
                .GroupBy(p => p.currency_code)
                .ToDictionary(g => g.Key, g => g.Select(p => p.product_id).OrderBy(id => id).ToList());
            bool onlyOneCurrency = idsGroupedByCurrency.Keys.Count == 1;
            bool onlyInRubCurrency = onlyOneCurrency && idsGroupedByCurrency.Keys.First() == "RUB";

            string idsArray = string.Join(",", source.Select(d => d.product_id).Distinct().OrderBy(id => id));

            if (source.Count == 0) { return; }

            var sb = new StringBuilder()
                .AppendLine("UPDATE oc_product")
                .AppendLine("SET base_price = CASE product_id");

            foreach (var productInfo in source)
            {
                sb.AppendLine($"WHEN '{productInfo.product_id}' THEN '{productInfo.price.Value.ToString(new CultureInfo("en-EN"))}'");
            }

            sb.AppendLine("ELSE base_price")
              .AppendLine("END, date_modified = NOW()");

            if (onlyOneCurrency)
            {
                sb.AppendLine($", base_currency_code = '{idsGroupedByCurrency.Keys.First()}'");
                if (onlyInRubCurrency)
                {
                    sb.AppendLine($", price = base_price");
                }
            }
            sb.AppendLine($"WHERE product_id IN ({idsArray});");

            if (!onlyOneCurrency)
            {
                //Обновляем тип валюты товара
                foreach (var kvp in idsGroupedByCurrency)
                {
                    string currencyIdsArray = string.Join(",", kvp.Value.OrderBy(id => id));
                    sb.AppendLine($"UPDATE oc_product SET base_currency_code = '{kvp.Key}' WHERE product_id IN ({currencyIdsArray});");
                }
            }

            var sql = sb.ToString();

            await database.SaveData<dynamic>(sql, new { });
        }

        public async Task UpdateProductsStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate)
        {
            List<ProductUpdateData> source = data.Where(d => d.quantity.HasValue && d.stock_partner == null).ToList();

            if (source.Count == 0) { return; }

            string pidArray = string.Join(",", source.Select(ud => ud.product_id).OrderBy(pid => pid).Distinct());

            var sb = new StringBuilder()
                .AppendLine("UPDATE oc_product")
                .AppendLine("SET quantity = CASE product_id");

            foreach (var productInfo in source)
            {
                sb.AppendLine($"WHEN '{productInfo.product_id}' THEN '{Math.Max(productInfo.quantity.Value, 0)}'");
            }

            sb.AppendLine("ELSE quantity")
              .AppendLine("END, date_modified = NOW()")
              .AppendLine($"WHERE product_id IN ({pidArray});");

            if (clearStockBeforeUpdate)
            {
                var clearStockQueryBuilder = new StringBuilder()
                    .AppendLine("UPDATE oc_product")
                    .AppendLine("SET quantity = 0")
                    .AppendLine($"WHERE manufacturer_id IN (SELECT DISTINCT manufacturer_id FROM oc_product WHERE product_id IN ({pidArray}));");

                sb.Insert(0, clearStockQueryBuilder.ToString());
            }


            string sql = sb.ToString();

            await database.SaveData<dynamic>(sql, new { });
        }

        public async Task UpdateProductsStockPartner(List<ProductUpdateData> data)
        {
            var source = data.Where(d => d.stock_partner != null).ToList();
            if (source.Any())
            {
                var groupedByPartner = source.GroupBy(line => line.stock_partner.Value);
                var partnerIdArray = string.Join(",", groupedByPartner.Select(g => g.Key).Distinct().ToList());

                var sb = new StringBuilder()
                    .AppendLine($"DELETE FROM oc_product_to_stock WHERE stock_partner_id IN ({partnerIdArray});")
                    .AppendLine("INSERT INTO oc_product_to_stock (stock_partner_id, product_id, quantity) VALUES");

                foreach (var group in groupedByPartner)
                {
                    foreach (var kvp in group)
                    {
                        sb.AppendLine($"({group.Key}, {kvp.product_id}, {kvp.quantity.Value}),");
                    }
                    if (group != groupedByPartner.Last())
                    {
                        sb.Append("\n");
                    }
                }

                var sql = sb.ToString().Trim('\r', '\n', ',');

                await database.SaveData<dynamic>(sql, new { });
            }
        }

        private async Task<List<int>> GetDiscountProductIds()
        {
            var sql = "SELECT DISTINCT product_id FROM oc_product_special WHERE NOW() BETWEEN date_start AND date_end";
            var list = (await database.LoadData<int, dynamic>(sql, new { })).ToList();
            return list;
        }

    }
}
