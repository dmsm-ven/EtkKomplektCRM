﻿using EtkBlazorApp.DataAccess.Entity;
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
        Task UpdateProductsStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate);
        Task UpdateProductsStockPartner(List<ProductUpdateData> data);
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

        private async Task<List<int>> GetDiscountProductIds()
        {
            var sql = "SELECT DISTINCT product_id FROM oc_product_special WHERE NOW() BETWEEN date_start AND date_end";
            var list = (await database.LoadData<int, dynamic>(sql, new { })).ToList();
            return list;
        }
    }
}
