using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Integration.Ozon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface IDatabaseProductCorrelator
    {
        Task<List<ProductUpdateData>> GetCorrelationData(IEnumerable<ProductEntity> products, IEnumerable<PriceLine> priceLines);
    }

    public class SimpleDatabaseProductCorrelator : IDatabaseProductCorrelator
    {
        public async Task<List<ProductUpdateData>> GetCorrelationData(IEnumerable<ProductEntity> products, IEnumerable<PriceLine> priceLines)
        {
            var list = new List<ProductUpdateData>();

            var validBrands = priceLines
                .GroupBy(p => p.Manufacturer)
                .Select(g => g.Key)
                .ToList();

            var productsWithValidBrand = products
                .Where(p => validBrands.Any(brand => brand.Equals(p.manufacturer, StringComparison.OrdinalIgnoreCase)))
                .ToList();


            await Task.Run(() =>
            {
                foreach (var product in productsWithValidBrand)
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
                    currency_code = priceLine.Currency.ToString(),
                    quantity = priceLine.Quantity
                };
            }

            return null;
        }
    }

    public class HardOrdereSkuModelProductCorrelator : IDatabaseProductCorrelator
    {
        public async Task<List<ProductUpdateData>> GetCorrelationData(IEnumerable<ProductEntity> products, IEnumerable<PriceLine> priceLines)
        {
            var list = new List<ProductUpdateData>();
            await Task.Run(() =>
            {
                foreach (var product in products)
                {
                    var findedLine = priceLines.FirstOrDefault(pl => pl.Sku.Equals(product.sku) || pl.Model.Equals(product.model));
                    if (findedLine != null)
                    {
                        var updateData = new ProductUpdateData()
                        {
                            product_id = product.product_id,
                            price = findedLine.Price,
                            currency_code = findedLine.Currency.ToString(),
                            quantity = findedLine.Quantity
                        };
                        list.Add(updateData);
                    }
                }
            });

            return list;
        }
    }

    
}
