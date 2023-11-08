using EtkBlazorApp.BL.Helpers;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class DictionaryCompareProductCorrelator : IDatabaseProductCorrelator
    {
        public async Task<List<ProductUpdateData>> GetCorrelationData(IEnumerable<ProductEntity> products, IEnumerable<PriceLine> priceLines)
        {
            var list = new List<ProductUpdateData>();

            var dictionaries = await Task.Run(() => PrepareDictionaries(priceLines));

            foreach (var product in products)
            {
                var findedLine =
                    dictionaries["sku"].GetValueOrNull(product.sku) ??
                    dictionaries["model"].GetValueOrNull(product.model) ??
                    dictionaries["ean"].GetValueOrNull(product.ean);

                if (findedLine != null)
                {
                    var updateData = new ProductUpdateData()
                    {
                        product_id = product.product_id,
                        price = findedLine.Price,
                        original_price = findedLine.OriginalPrice,
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

            return list;
        }

        private Dictionary<string, Dictionary<string, PriceLine>> PrepareDictionaries(IEnumerable<PriceLine> priceLines)
        {

            Dictionary<string, PriceLine> eanDic = null;
            bool useEan = priceLines.Any(p => !string.IsNullOrWhiteSpace(p.Ean));
            if (useEan)
            {
                eanDic = priceLines
                    .Where(p => !string.IsNullOrWhiteSpace(p.Ean))
                    .GroupBy(p => p.Ean)
                    .Select(g => new
                    {
                        EAN = g.Key,
                        Product = g.FirstOrDefault()
                    })
                    .Where(item => item.Product != null)
                    .ToDictionary(i => i.EAN, i => i.Product);
            }

            //Dictionary<string, PriceLine> nameDic = null;
            //bool useName = priceLines.Any(p => !string.IsNullOrWhiteSpace(p.Name));
            //if (useName)
            //{
            //    nameDic = priceLines
            //        .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            //        .GroupBy(p => p.Name)
            //        .Select(g => new
            //        {
            //            Name = g.Key,
            //            Product = g.FirstOrDefault()
            //        })
            //        .Where(item => item.Product != null)
            //        .ToDictionary(i => i.Name, i => i.Product);
            //}

            Dictionary<string, PriceLine> skuDic = priceLines
                .Where(p => !string.IsNullOrWhiteSpace(p.Sku))
                .GroupBy(p => p.Sku)
                .Select(g => new
                {
                    Sku = g.Key,
                    Product = g.FirstOrDefault()
                })
                .Where(item => item.Product != null)
                .ToDictionary(i => i.Sku, i => i.Product);

            Dictionary<string, PriceLine> modelDic = priceLines
                .Where(p => !string.IsNullOrWhiteSpace(p.Model))
                .GroupBy(p => p.Model)
                .Select(g => new
                {
                    Model = g.Key,
                    Product = g.FirstOrDefault()
                })
                .Where(item => item.Product != null)
                .ToDictionary(i => i.Model, i => i.Product);

            return new Dictionary<string, Dictionary<string, PriceLine>>()
            {
                ["sku"] = skuDic,
                ["model"] = modelDic,
                ["ean"] = eanDic
            };
        }
    }
}
