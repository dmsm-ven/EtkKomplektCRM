using EtkBlazorApp.Data;
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
        ProductEntity GetProductById(int id);
        Task UpdateStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate);
        Task UpdatePrice(List<ProductUpdateData> data);
        Task<List<ProductEntity>> ReadProducts();
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
                .AppendLine("LEFT JOIN oc_product_description d ON p.product_id = d.product_id")
                .AppendLine("LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                .AppendLine("ORDER BY Date(p.date_added) DESC")
                .AppendLine("LIMIT @Limit");

            string sql = sb.ToString();

            var products = await database.LoadData<ProductEntity, dynamic>(sql, new { Limit = count });

            return products.ToList();
        }

        public ProductEntity GetProductById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ProductEntity>> ReadProducts()
        {
            var sb = new StringBuilder()
                    .AppendLine("SELECT p.*, d.name as name, m.name as manufacturer, url.keyword as url")
                    .AppendLine("FROM oc_product p")
                    .AppendLine("LEFT JOIN oc_product_description d ON p.product_id = d.product_id")
                    .AppendLine("LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                    .AppendLine("LEFT JOIN oc_url_alias url ON CONCAT('product_id=', p.product_id) = url.query");

            string sql = sb.ToString().Trim(); 

            var products = await database.LoadData<ProductEntity, dynamic>(sql, new { });

            return products;

        }

        public async Task UpdatePrice(List<ProductUpdateData> data)
        {
            List<int> discountProductIds = await GetDiscountProductIds();
            
            List<ProductUpdateData> source = data
                .Where(d => d.price.HasValue && !discountProductIds.Contains(d.product_id))
                .ToList();

            Dictionary<CurrencyType, List<int>> idsGroupedByCurrency = source
                .GroupBy(p => p.currency_code)
                .ToDictionary(g => g.Key, g => g.Select(p => p.product_id).OrderBy(id => id).ToList());
            bool onlyOneCurrency = idsGroupedByCurrency.Keys.Count == 1;
 
            string idsArray = string.Join(",", source.Select(d => d.product_id).Distinct().OrderBy(id => id));

            if (source.Count == 0) { return; }

            var sb = new StringBuilder()
                .AppendLine("UPDATE oc_product")
                .AppendLine("SET base_price = CASE product_id");

                foreach (var productInfo in source)
                {
                    sb.AppendLine($"WHEN '{productInfo.product_id}' THEN '{productInfo.price.Value.ToString(new CultureInfo("en-EN"))}'");
                }

                sb.AppendLine("ELSE base_price").AppendLine("END, date_modified = NOW()");

                if (onlyOneCurrency)
                {
                    sb.AppendLine($", base_currency_code = '{idsGroupedByCurrency.Keys.First()}'");
                }
                sb.AppendLine($"WHERE product_id IN ({idsArray});");

                if (!onlyOneCurrency)
                {
                    //Обновляем тип валюты товара
                    foreach (var kvp in idsGroupedByCurrency)
                    {
                        string currencyIdsArray = string.Join(",", source.Select(d => d.product_id).Distinct().OrderBy(id => id));
                        sb.AppendLine($"UPDATE oc_product SET base_currency_code = '{kvp.Key}' WHERE product_id IN ({currencyIdsArray});");
                    }
                }

            var sql = sb.ToString();

            await database.SaveData<dynamic>(sql, new { });
        }

        public async Task UpdateStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate)
        {
            List<ProductUpdateData> source = data
                .Where(d => d.quantity.HasValue)
                .ToList();

            if (source.Count == 0) { return; }

            var sb = new StringBuilder()
                .AppendLine("UPDATE oc_product")
                .AppendLine("SET quantity = CASE product_id");

            foreach (var productInfo in source)
            {
                sb.AppendLine($"WHEN '{productInfo.product_id}' THEN '{productInfo.quantity}'");
            }

            sb.AppendLine("ELSE quantity")
              .AppendLine("END, date_modified = NOW();");

            if (clearStockBeforeUpdate)
            {
                string idsArray = string.Join(",", source.Select(d => d.product_id).Distinct().OrderBy(id => id));
                var clearStockQueryBuilder = new StringBuilder()
                    .AppendLine("UPDATE oc_product")
                    .AppendLine("SET quantity = 0")
                    .AppendLine($"WHERE manufacturer_id IN (SELECT manufacturer_id FROM oc_manufacturer WHERE product_id IN ({idsArray}));");
                

                sb.Insert(0, clearStockQueryBuilder.ToString());
            }

            string sql = sb.ToString();

            await database.SaveData<dynamic>(sql, new { });
        }

        /// <summary>
        /// Получение списка ID товаров которые участвуют акции на данный момент
        /// </summary>
        /// <returns></returns>
        private async Task<List<int>> GetDiscountProductIds()
        {
            var sql = "SELECT DISTINCT product_id FROM oc_product_special WHERE NOW() BETWEEN date_start AND date_end";
            var list = (await database.LoadData<int, dynamic>(sql, new { })).ToList();
            return list;
        }
    }
}
