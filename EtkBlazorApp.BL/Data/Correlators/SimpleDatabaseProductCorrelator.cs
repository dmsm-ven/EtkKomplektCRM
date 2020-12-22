using EtkBlazorApp.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Data
{
    public class SimpleDatabaseProductCorrelator : IDatabaseProductCorrelator
    {
        public async Task<List<ProductUpdateData>> GetCorrelationData(List<ProductEntity> products, List<PriceLine> priceLines)
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

        private ProductUpdateData GetCorrelationDataForProduct(ProductEntity product, List<PriceLine> priceLines)
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
                    currency_code = priceLine.Currency.ToString(),
                    quantity = priceLine.Quantity
                };
            }

            return null;
        }
    }
}
