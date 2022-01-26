using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IProductUpdateService
    {
        Task UpdateProductsPrice(List<ProductUpdateData> data);
        Task UpdateProductsStock(List<ProductUpdateData> data);
        Task UpdateNextStockDelivery(List<ProductUpdateData> data);
        Task UpdateProductsStockPartner(List<ProductUpdateData> data, int [] affectedBrands);
        Task ComputeStockQuantity(List<ProductUpdateData> data);
        Task UpdateDirectProduct(ProductEntity product);
    }

    public class ProductUpdateService : IProductUpdateService
    {
        private readonly IDatabaseAccess database;

        public ProductUpdateService(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task UpdateProductsPrice(List<ProductUpdateData> data)
        {
            //Если товар в акции или в специальном списке то не обновляем его
            List<int> skipProducts = await GetSkipProductIds();

            List<ProductUpdateData> source = data
                .OrderBy(p => p.product_id)
                .Where(d => d.price.HasValue && skipProducts.Contains(d.product_id) == false)
                .ToList();

            if (source.Count == 0) { return; }

            Dictionary<string, List<int>> idsGroupedByCurrency = source
                .GroupBy(p => p.currency_code)
                .ToDictionary(g => g.Key, g => g.Select(p => p.product_id).OrderBy(id => id).ToList());
            bool onlyOneCurrency = idsGroupedByCurrency.Keys.Count == 1;
            bool onlyInRubCurrency = onlyOneCurrency && idsGroupedByCurrency.Keys.First() == "RUB";

            string idsArray = string.Join(",", source.Select(d => d.product_id).Distinct().OrderBy(id => id));

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

            await database.ExecuteQuery(sql);
        }

        public async Task UpdateProductsStock(List<ProductUpdateData> data)
        {
            List<ProductUpdateData> source = data
                .OrderBy(p => p.product_id)
                .Where(d => d.quantity.HasValue && d.stock_id != 0)
                .ToList();

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


            string sql = sb.ToString();

            await database.ExecuteQuery<dynamic>(sql, new { });
        }

        public async Task UpdateProductsStockPartner(List<ProductUpdateData> source, int[] affectedBrands)
        {           
            source = source
                .OrderBy(p => p.product_id)
                .Where(item => item.quantity.HasValue)
                .ToList();

            if (source.Any())
            {
                var groupedByPartner = source
                    .GroupBy(line => line.stock_id)
                    .ToDictionary(i => i.Key, j => j.ToList());

                //Дополнительные склады
                if (source.Any(i => i.AdditionalStocksQuantity != null))
                { 
                    AppendQuantityFromAdditionalStocks(source, groupedByPartner);
                }

                var partnerIdArray = string.Join(",", groupedByPartner.Select(g => g.Key).Distinct());
                var affectedBrandIdsArray = string.Join(",", affectedBrands);

                string clearStockSql = $@"UPDATE oc_product_to_stock
                                          JOIN oc_product ON oc_product.product_id = oc_product_to_stock.product_id
                                          JOIN oc_manufacturer ON oc_product.manufacturer_id = oc_manufacturer.manufacturer_id
                                          SET oc_product_to_stock.quantity = 0
                                          WHERE oc_product_to_stock.stock_partner_id IN ({partnerIdArray}) AND oc_manufacturer.manufacturer_id IN ({affectedBrandIdsArray})";
                await database.ExecuteQuery(clearStockSql);


                var sb = new StringBuilder("INSERT INTO oc_product_to_stock (stock_partner_id, product_id, quantity) VALUES\n");
                foreach (var group in groupedByPartner)
                {
                    foreach (var kvp in group.Value)
                    {
                        sb.AppendLine($"({group.Key}, {kvp.product_id}, {kvp.quantity.Value}),");
                    }
                }
                var sql = sb.ToString().Trim('\r', '\n', ',') + " ON DUPLICATE KEY UPDATE quantity = VALUES(quantity)";
                await database.ExecuteQuery(sql);
            }
        }

        private void AppendQuantityFromAdditionalStocks(List<ProductUpdateData> source, Dictionary<int, List<ProductUpdateData>> groupedByPartner)
        {          
            foreach (var item in source.Where(i => i.AdditionalStocksQuantity != null))
            {
                foreach (var kvp in item.AdditionalStocksQuantity)
                {
                    if (!groupedByPartner.ContainsKey(kvp.Key))
                    {
                        groupedByPartner.Add(kvp.Key, new List<ProductUpdateData>());
                    }
                    groupedByPartner[kvp.Key].Add(new ProductUpdateData() { product_id = item.product_id, quantity = kvp.Value });
                }
            }
        }

        public async Task ComputeStockQuantity(List<ProductUpdateData> data)
        {
            var idsToUpdate = data
                .Where(d => d.quantity.HasValue)
                .Select(d => d.product_id)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            var pidArray = string.Join(",", idsToUpdate);

            if (idsToUpdate.Any())
            {
                string pileUpStockSql = $@"UPDATE oc_product
                                           SET quantity = GREATEST(0, (SELECT SUM(oc_product_to_stock.quantity) 
                                                                      FROM oc_product_to_stock 
                                                                      WHERE oc_product_to_stock.product_id = oc_product.product_id))
                                           WHERE product_id IN ({pidArray})";
                await database.ExecuteQuery(pileUpStockSql);
            }
        }

        public async Task UpdateDirectProduct(ProductEntity product)
        {
            string sql = @"UPDATE oc_product
                        SET price = @price,
                            base_price = @base_price,
                            base_currency_code = @base_currency_code,
                            quantity = @quantity,
                            stock_status_id = (SELECT stock_status_id FROM oc_stock_status WHERE name = @stock_status),
                            date_modified = NOW()
                            WHERE product_id = @product_id";

            await database.ExecuteQuery(sql, product);

            if (product.replacement_id.HasValue)
            {
                string addReplacementSql = @"INSERT INTO oc_product_replacement (product_id, replacement_id)
                                             VALUES (@product_id, @replacement_id)
                                             ON DUPLICATE KEY
                                             UPDATE replacement_id = @replacement_id";

                await database.ExecuteQuery(addReplacementSql, product);
            }
            else
            {
                await database.ExecuteQuery("DELETE FROM oc_product_replacement WHERE product_id = @product_id", product);
            }
        }

        public async Task UpdateNextStockDelivery(List<ProductUpdateData> data)
        {
            var source = data.Where(pl => pl.NextStockDelivery != null).ToList();
            
            if(source.Count == 0) { return; }

            string stock_ids = string.Join(",", source.GroupBy(p => p.stock_id).Select(g => g.Key));
            await database.ExecuteQuery($"DELETE FROM oc_stock_next_delivery WHERE stock_id IN ({stock_ids})");

            var sb = new StringBuilder();

            sb.AppendLine("INSERT INTO oc_stock_next_delivery (stock_id, product_id, quantity, next_shipment_date) VALUES");
            foreach(var line in source)
            {
                sb.AppendLine($"({line.stock_id}, {line.product_id}, {line.NextStockDelivery.Quantity}, '{line.NextStockDelivery.Date.ToString("yyyy-MM-dd")}'),");
            }

            string sql = sb.ToString().Trim('\r', '\n', '\t', ' ', ',') + ";";
            await database.ExecuteQuery(sql);

        }

        private async Task<List<int>> GetSkipProductIds()
        {
            List<int> discountProductIds = await GetDiscountProductIds();
            List<int> fixedProductIds = await GetFixedProductIds();

            return discountProductIds.Concat(fixedProductIds).Distinct().ToList();
        }

        private async Task<List<int>> GetDiscountProductIds()
        {
            var sql = "SELECT DISTINCT product_id FROM oc_product_special WHERE NOW() BETWEEN date_start AND date_end";
            var ids = await database.GetList<int>(sql);
            return ids;
        }

        private async Task<List<int>> GetFixedProductIds()
        {
            var sql = "SELECT product_id FROM etk_app_fixed_product";
            var ids = await database.GetList<int>(sql);
            return ids;
        }
    }
}
