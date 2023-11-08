using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class FullCompareProductCorrelator : IDatabaseProductCorrelator
    {
        public async Task<List<ProductUpdateData>> GetCorrelationData(IEnumerable<ProductEntity> products, IEnumerable<PriceLine> priceLines)
        {
            var bySku = new Func<ProductEntity, PriceLine>((p) => string.IsNullOrWhiteSpace(p.sku) ? null : priceLines.FirstOrDefault(line => p.sku.Equals(line.Sku)));
            var byModel = new Func<ProductEntity, PriceLine>((p) => string.IsNullOrWhiteSpace(p.model) ? null : priceLines.FirstOrDefault(line => p.model.Equals(line.Model)));
            var byEan = new Func<ProductEntity, PriceLine>((p) => string.IsNullOrWhiteSpace(p.ean) ? null : priceLines.FirstOrDefault(line => p.ean.Equals(line.Ean)));
            var byName = new Func<ProductEntity, PriceLine>((p) => string.IsNullOrWhiteSpace(p.name) ? null : priceLines.FirstOrDefault(line => p.name.Equals(line.Name)));

            var list = new List<ProductUpdateData>();
            await Task.Run(() =>
            {
                foreach (var product in products)
                {
                    var findedLine = new[] { byEan(product), bySku(product), byModel(product), byName(product) }
                        .FirstOrDefault(v => v != null);

                    if (findedLine != null)
                    {
                        var updateData = new ProductUpdateData()
                        {
                            product_id = product.product_id,
                            price = findedLine.Price,
                            currency_code = findedLine.Currency,
                            quantity = findedLine.Quantity,
                            stock_id = (int)findedLine.Stock
                        };

                        if (findedLine is MultistockPriceLine)
                        {
                            updateData.AdditionalStocksQuantity = (findedLine as MultistockPriceLine)
                                .AdditionalStockQuantity
                                .ToDictionary(i => (int)i.Key, i => i.Value);
                        }

                        if (findedLine is PriceLineWithNextDeliveryDate)
                        {
                            updateData.NextStockDelivery = (findedLine as PriceLineWithNextDeliveryDate).NextStockDelivery;
                        }

                        list.Add(updateData);
                    }
                }
            });

            return list;
        }
    }
}
