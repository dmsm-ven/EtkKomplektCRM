using EtkBlazorApp.Data;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Correlators
{
    public class SimpleDatabaseProductCorrelator : IDatabaseProductCorrelator
    {
        public async Task<List<ProductUpdateData>> GetCorrelationData(IEnumerable<ProductEntity> products, IEnumerable<PriceLine> priceLines)
        {
            var list = new List<ProductUpdateData>();

            await Task.Run(() =>
            {
                foreach (var product in products)
                {
                    var correlationData = GetCorrelationDataForProduct(product, priceLines);
                    if (correlationData != null)
                    {
                        list.Add(correlationData);
                    }
                }
            });

            return list;
        }

        private ProductUpdateData GetCorrelationDataForProduct(ProductEntity product, IEnumerable<PriceLine> priceLines)
        {
            PriceLine priceLine = null;
            if (!string.IsNullOrWhiteSpace(product.sku))
            {
                priceLine = priceLines.FirstOrDefault(line => line.Sku.Equals(product.sku, StringComparison.OrdinalIgnoreCase));
            }

            if (priceLine == null)
            {
                priceLine = priceLines
                    .Where(line => !string.IsNullOrWhiteSpace(line.Model))
                    .FirstOrDefault(line => line.Model.Equals(product.model, StringComparison.OrdinalIgnoreCase));

                if (priceLine == null)
                {
                    priceLine = priceLines
                        .Where(line =>
                            ((!string.IsNullOrWhiteSpace(product.sku) && !string.IsNullOrWhiteSpace(line.Model) && line.Model.Equals(product.sku, StringComparison.OrdinalIgnoreCase)))
                            ||
                            line.Sku.Equals(product.model, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();
                }
            }

            if (priceLine != null)
            {
                return new ProductUpdateData()
                {
                    product_id = product.product_id,
                    price = priceLine.Price,
                    currency_code = priceLine.Currency,
                    quantity = priceLine.Quantity
                };
            }

            return null;
        }
    }
}
