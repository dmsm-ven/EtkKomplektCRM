using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.Model.Chart
{
    public class ChartDataExtractor
    {
        private readonly IOrderStorage ordersStorage;
        private readonly IPriceListUpdateHistoryRepository productPriceHistoryRepo;

        public ChartDataExtractor(IOrderStorage ordersStorage, IPriceListUpdateHistoryRepository productPriceHistoryRepo)
        {
            this.ordersStorage = ordersStorage;
            this.productPriceHistoryRepo = productPriceHistoryRepo;
        }

        public async Task<ProductPriceDynamicChartData> GetDataSourceForProductPriceDynamic(int product_id)
        {
            var data = await productPriceHistoryRepo.GetPriceDynamicForProduct(product_id);

            var grouped = data.GroupBy(i => i.price_list_title)
                .Select(i => new ProductPriceDynamicChartDataEntry()
                {
                    PriceListTitle = i.Key,
                    PriceByDate = i.OrderByDescending(i => i.date_time).ToDictionary(p => p.date_time, p => p.price)
                })
                .ToList();


            return new ProductPriceDynamicChartData()
            {
                ByPriceListTitle = grouped
            };
        }

        public async Task<IReadOnlyDictionary<string, decimal>> GetDataSourceForOrdersChart(ChartDateRange period, ChartKind kind, int maxItems = 10)
        {
            DateTime startDate = GetStartDate(period);

            Dictionary<string, decimal> data = null;

            if (kind == ChartKind.ByCustomer)
            {
                var orders = await ordersStorage.GetOrdersFromDate(startDate);

                data = orders
                .GroupBy(order => Regex.Replace(order.telephone, @"[^\d]", "").TrimStart('8', '7'))
                .Select(g => new
                {
                    Customer = $"{g.First().firstname} (+7{g.Key})",
                    TotalSum = g.Sum(o => o.total)
                })
                .Where(g => g.Customer != null)
                .OrderByDescending(i => i.TotalSum)
                .Take(maxItems)
                .ToDictionary(i => i.Customer, j => j.TotalSum);
            }
            if (kind == ChartKind.ByCity)
            {
                var orders = await ordersStorage.GetOrdersFromDate(startDate);

                data = orders
                    .GroupBy(order => order.payment_zone, StringComparer.OrdinalIgnoreCase)
                        .Select(g => new
                        {
                            City = g.Key,
                            TotalSum = g.Sum(o => o.total)
                        })
                        .Where(g => g.City != null)
                        .OrderByDescending(i => i.TotalSum)
                        .Take(maxItems)
                        .ToDictionary(i => i.City, j => j.TotalSum);
            }
            if (kind == ChartKind.ByManufacturer)
            {
                var orders = await ordersStorage.GetOrderDetailsFromDate(startDate);

                data = orders
                .GroupBy(od => od.manufacturer, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Manufacturer = HttpUtility.HtmlDecode(g.Key),
                    TotalSum = g.Sum(o => o.total)
                })
                .Where(g => g.Manufacturer != null)
                .OrderByDescending(i => i.TotalSum)
                .Take(maxItems)
                .ToDictionary(i => i.Manufacturer, j => j.TotalSum);
            }
            if (kind == ChartKind.ByProduct)
            {
                var orders = await ordersStorage.GetOrderDetailsFromDate(startDate);

                data = orders
                    .GroupBy(od => od.name, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new
                    {
                        ProductName = HttpUtility.HtmlDecode(g.Key),
                        TotalSum = g.Sum(o => o.total)
                    })
                    .Where(g => g.ProductName != null)
                    .OrderByDescending(i => i.TotalSum)
                    .Take(maxItems)
                    .ToDictionary(i => i.ProductName, j => j.TotalSum);
            }

            return data;
        }

        private DateTime GetStartDate(ChartDateRange period)
        {
            int maxAge = 0;
            switch (period)
            {
                case ChartDateRange.Week: maxAge = 7; break;
                case ChartDateRange.Month: maxAge = 31; break;
                case ChartDateRange.Year: maxAge = 365; break;
                default: maxAge = 0; break;
            }

            return DateTime.Now.AddDays(-maxAge);

        }
    }

    public class ProductPriceDynamicChartData
    {
        public List<ProductPriceDynamicChartDataEntry> ByPriceListTitle { get; set; }
    }

    public class ProductPriceDynamicChartDataEntry
    {
        public string PriceListTitle { get; set; }
        public Dictionary<DateTime, decimal> PriceByDate { get; set; }
    }
}
