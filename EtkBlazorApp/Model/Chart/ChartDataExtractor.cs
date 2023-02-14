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
        const int MAX_HISTORY_AGE = 20;


        public ChartDataExtractor(IOrderStorage ordersStorage, IPriceListUpdateHistoryRepository productPriceHistoryRepo)
        {
            this.ordersStorage = ordersStorage;
            this.productPriceHistoryRepo = productPriceHistoryRepo;
        }

        public async Task<ProductPriceDynamicChartData> GetDataSourceForProductPriceDynamic(int product_id)
        {
            var data = await productPriceHistoryRepo.GetPriceDynamicForProduct(product_id);

            if (data.Count == 0)
            {
                return null;
            }

            var grouped = data.GroupBy(i => i.price_list_title)
                .Select(i => new ProductPriceDynamicChartDataEntry()
                {
                    PriceListTitle = i.Key,
                    PriceByDate = i.OrderByDescending(i => i.date_time).ToDictionary(p => p.date_time, p => p.price)
                })
                .ToList();

            if (grouped.Count == 0)
            {
                return null;
            }


            var colors = new string[4] { "#ff0000", "#0000ff", "#000000", "#ffff00" };

            var seriesData = new List<ChartSeriesData>();
            var xAxisLabels = grouped
                .SelectMany(i => i.PriceByDate.Keys)
                .OrderByDescending(date => date)
                .Select(i => i.ToString("dd.MM.yyyy"))
                .Distinct()
                .Take(MAX_HISTORY_AGE)
                .ToArray();

            int i = 0;
            foreach (var kvp in grouped)
            {
                List<decimal> values = new List<decimal>();
                Dictionary<string, decimal> stringDates = kvp
                    .PriceByDate
                    .GroupBy(kvp => kvp.Key.ToString("dd.MM.yyyy"))
                    .ToDictionary(j => j.Key, j => j.First().Value);

                foreach (var d in xAxisLabels)
                {
                    if (stringDates.TryGetValue(d, out var price))
                    {
                        values.Add(price);
                    }
                    else if (values.Any())
                    {
                        values.Add(values.Last());
                    }
                    else
                    {
                        values.Add(kvp.PriceByDate.Values.First());
                    }

                }

                var priceListSeries = new ChartSeriesData
                {
                    label = kvp.PriceListTitle,
                    data = values.ToArray(),
                    fill = false,
                    borderColor = colors[i++],
                    tension = 0.1
                };

                seriesData.Add(priceListSeries);
            }


            return new ProductPriceDynamicChartData()
            {
                XAxisLabels = xAxisLabels,
                SeriesData = seriesData.ToArray()
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
        public ChartSeriesData[] SeriesData { get; set; }
        public string[] XAxisLabels { get; set; }
    }

    public class ChartSeriesData
    {
        public string label { get; set; }
        public decimal[] data { get; set; }
        public bool fill { get; set; }
        public string borderColor { get; set; }
        public double tension { get; set; }
    }

    public class ProductPriceDynamicChartDataEntry
    {
        public string PriceListTitle { get; set; }
        public Dictionary<DateTime, decimal> PriceByDate { get; set; }
    }
}
