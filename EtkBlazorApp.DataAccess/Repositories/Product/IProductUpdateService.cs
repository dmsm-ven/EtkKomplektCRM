using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IProductUpdateService
    {
        Task UpdateNextStockDelivery(IEnumerable<ProductUpdateData> data);
        Task UpdateDirectProduct(ProductEntity product);

        Task UpdateStockProducts(IEnumerable<ProductUpdateData> data, int[] affectedBrands);
        Task ComputeStocksQuantity(IEnumerable<ProductUpdateData> data);
        Task ComputeProductsPrice(IEnumerable<ProductUpdateData> data);
    }

    public class ProductUpdateService : IProductUpdateService
    {
        private readonly IDatabaseAccess database;

        public ProductUpdateService(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task UpdateStockProducts(IEnumerable<ProductUpdateData> source, int[] affectedBrands)
        {
            if (source.Count() == 0)
            {
                return;
            }

            var orderedSource = source
                .OrderBy(p => p.product_id)
                .ToList();

            var groupedByPartner = orderedSource
                .GroupBy(line => line.stock_id)
                .ToDictionary(i => i.Key, j => j.ToList());

            //Дополнительные склады
            if (source.Any(i => i.AdditionalStocksQuantity != null))
            {
                AppendToAdditionalStocks(orderedSource, groupedByPartner);
            }

            await UpdateProductsStockQuantity(groupedByPartner);

            await UpdateProductsStockPrice(groupedByPartner);
        }

        private async Task UpdateProductsStockQuantity(IReadOnlyDictionary<int, List<ProductUpdateData>> groupedByPartner)
        {
            if (!groupedByPartner.Any(kvp => kvp.Value.Count(i => i.quantity.HasValue) > 0))
            {
                return;
            }


            var sb = new StringBuilder("INSERT INTO oc_product_to_stock (stock_partner_id, product_id, quantity) VALUES\n");
            foreach (var group in groupedByPartner)
            {
                foreach (var kvp in group.Value.Where(v => v.quantity.HasValue))
                {
                    sb.AppendLine($"({group.Key}, {kvp.product_id}, {Math.Max(kvp.quantity.Value, 0)}),");
                }
            }
            var sql = sb.ToString().Trim('\r', '\n', ',') + " ON DUPLICATE KEY UPDATE quantity = VALUES(quantity);";

            await database.ExecuteQuery(sql);
        }

        private async Task UpdateProductsStockPrice(IReadOnlyDictionary<int, List<ProductUpdateData>> groupedByPartner)
        {
            if (!groupedByPartner.Any(kvp => kvp.Value.Count(i => i.price.HasValue && i.original_price.HasValue) > 0))
            {
                return;
            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var sb = new StringBuilder("INSERT INTO oc_product_to_stock (stock_partner_id, product_id, original_price, price, currency_code) VALUES\n");
            foreach (var group in groupedByPartner)
            {
                foreach (var kvp in group.Value)
                {
                    sb.AppendLine($"({group.Key}, {kvp.product_id}, '{kvp.original_price}', '{kvp.price}', '{kvp.currency_code}'),");
                }
            }
            var sql = sb.ToString().Trim('\r', '\n', ',') + @" ON DUPLICATE KEY UPDATE 
                                                                    original_price = VALUES(original_price), 
                                                                    price = VALUES(price), 
                                                                    currency_code = VALUES(currency_code);";
            await database.ExecuteQuery(sql);
        }

        private void AppendToAdditionalStocks(IEnumerable<ProductUpdateData> source, Dictionary<int, List<ProductUpdateData>> groupedByPartner)
        {
            foreach (var item in source.Where(i => i.AdditionalStocksQuantity != null))
            {
                foreach (var kvp in item.AdditionalStocksQuantity)
                {
                    if (!groupedByPartner.ContainsKey(kvp.Key))
                    {
                        groupedByPartner.Add(kvp.Key, new List<ProductUpdateData>());
                    }
                    groupedByPartner[kvp.Key].Add(new ProductUpdateData()
                    {
                        product_id = item.product_id,
                        quantity = kvp.Value,
                        original_price = item.original_price,
                        price = item.price,
                        currency_code = item.currency_code
                    });
                }
            }
        }

        public async Task ComputeProductsPrice(IEnumerable<ProductUpdateData> data)
        {
            var idsToUpdate = data
                .Where(d => d.price.HasValue)
                .Select(d => d.product_id)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            var pidArray = string.Join(",", idsToUpdate);

            string pileUpStockSql = $@"UPDATE oc_product
                                       INNER JOIN (SELECT pts.*, MIN(pts.price) as min_price
      		                                       FROM oc_product_to_stock as pts
      		                                       JOIN oc_currency as curr ON (`pts`.`currency_code` = `curr`.`code`)
			                                       WHERE product_id IN ({pidArray})
                                                   GROUP BY pts.product_id) as inner_tbl
                                       SET oc_product.price = inner_tbl.min_price, 
	                                       oc_product.base_price = inner_tbl.price,
                                           oc_product.base_currency_code = inner_tbl.currency_code,
                                           oc_product.date_modified = NOW()
                                       WHERE oc_product.product_id = inner_tbl.product_id";
            await database.ExecuteQuery(pileUpStockSql);
        }

        public async Task ComputeStocksQuantity(IEnumerable<ProductUpdateData> data)
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

        public async Task UpdateNextStockDelivery(IEnumerable<ProductUpdateData> data)
        {
            var source = data.Where(pl => pl.NextStockDelivery != null).ToList();

            if (source.Count == 0) { return; }

            string stock_ids = string.Join(",", source.GroupBy(p => p.stock_id).Select(g => g.Key));
            await database.ExecuteQuery($"DELETE FROM oc_stock_next_delivery WHERE stock_id IN ({stock_ids})");

            var sb = new StringBuilder();

            sb.AppendLine("INSERT INTO oc_stock_next_delivery (stock_id, product_id, quantity, next_shipment_date) VALUES");
            foreach (var line in source)
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
