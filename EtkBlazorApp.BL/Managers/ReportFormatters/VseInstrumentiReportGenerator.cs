using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;
using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;
using EtkBlazorApp.BL.Templates.PrikatTemplates;
using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Entity.Marketplace;
using EtkBlazorApp.DataAccess.Repositories;
using EtkBlazorApp.DataAccess.Repositories.Product;
using Humanizer;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers.ReportFormatters
{
    public class VseInstrumentiReportGenerator
    {
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        private readonly ICurrencyChecker currencyChecker;
        private readonly IPrikatTemplateStorage templateStorage;
        private readonly IProductStorage productStorage;
        private readonly IStockStorage stockStorage;

        public VseInstrumentiReportGenerator(ICurrencyChecker currencyChecker,
            IPrikatTemplateStorage templateStorage,
            IProductStorage productStorage,
            IStockStorage stockStorage)
        {
            this.currencyChecker = currencyChecker;
            this.templateStorage = templateStorage;
            this.productStorage = productStorage;
            this.stockStorage = stockStorage;
        }

        /// <summary>
        /// Создает отчет для ВсеИнструменты для включенных (enabled) брендов
        /// </summary>
        /// <param name="selectedTemplateIds"></param>
        /// <param name="inStock"></param>
        /// <param name="hasEan"></param>
        /// <returns>Ссылку на созданный файле на сервере</returns>
        public async Task<string> Create(VseInstrumentiReportOptions options)
        {
            Stopwatch sw = Stopwatch.StartNew();

            var templateSource = await templateStorage.GetPrikatTemplates(includeDisabled: false);

            DateTime generationDateTime = DateTime.Now;
            string extension = options.PricatFormat.ToString().ToLower();
            string document_id = PricatFormatterBase.GenerateDocumentNumber(generationDateTime);
            string fileName = $"pricat_{document_id}_{DateTime.Now.ToString("yyyyMMdd")}.{extension}";
            string filePath = Path.GetTempPath() + fileName;

            try
            {
                await StartExportFileAsync(filePath, options, templateSource, generationDateTime);

                nlog.Info("Выгрузка ВИ Pricat создана {fileName}. Длительность генерации файла: {elapsed}", filePath, sw.Elapsed.Humanize());
            }
            catch (Exception ex)
            {
                nlog.Error("Ошибка создания выгрузки для ВсеИнструменты. Детали: {stack}", ex.StackTrace);
                throw;
            }

            return filePath;
        }

        private async Task StartExportFileAsync(string fileName,
            VseInstrumentiReportOptions options,
            List<PrikatReportTemplateEntity> templateSource,
            DateTime generationDateTime)
        {
            await Task.Run(async () =>
            {
                var formatter = PricatFormatterFactory.Create(options);

                using (var fs = new FileStream(fileName, FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var data in templateSource)
                    {
                        PrikatReportTemplateBase template = await GetTemplate(data, options);
                        formatter.SetCurrentTemplate(template);

                        if (data == templateSource.First())
                        {
                            formatter.SetStreamWriter(sw);
                            formatter.OnDocumentStart(generationDateTime);
                        }

                        await InsertTemplateProductLines(data, template, formatter);
                    }
                    formatter.OnDocumentEnd();
                    sw.Flush();
                }
            });
        }

        private async Task InsertTemplateProductLines(PrikatReportTemplateEntity data,
            PrikatReportTemplateBase template,
            PricatFormatterBase formatter)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool shouldFillStockData = template.GetType() != typeof(PrikatDefaultReportTemplate);

            List<ProductEntity> products = await PrepareProducts(data, shouldFillStockData);

            template.WriteProductLines(products, formatter);

            nlog.Trace("Выгрузка товаров для бренда {brandName} ({productsCount}). Длительность загрузки для этого бренда: {elapsed}",
                data.manufacturer_name, products.Count, stopwatch.Elapsed.Humanize());
        }

        private async Task<PrikatReportTemplateBase> GetTemplate(PrikatReportTemplateEntity data, VseInstrumentiReportOptions options)
        {
            var currency = Enum.Parse<CurrencyType>(data.currency_code);
            decimal currentCurrencyRate = await currencyChecker.GetCurrencyRate(currency);

            PrikatReportTemplateBase template = PrikatReportTemplateFactory.Create(data.manufacturer_name, currency, data.discount, currentCurrencyRate, options);

            var discountedProducts = await templateStorage.GetDiscountedProducts();
            if (discountedProducts != null && discountedProducts.Count > 0)
            {
                template.AddDiscountMapItems(discountedProducts.Select(i => new KeyValuePair<int, decimal>(i.product_id, i.discount_price.Value)));
            }

            return template;
        }

        private async Task<List<ProductEntity>> PrepareProducts(PrikatReportTemplateEntity data, bool shouldFilStockData)
        {
            //Загружаем все товары для бренда
            List<ProductEntity> products = await productStorage.ReadProducts(data.manufacturer_id);

            //Получаем список всевозможных складов для товаров у этого бренда
            List<ProductToStockEntity> stockQuantityAll = await stockStorage.GetStockDataForProducts(products.Select(p => p.product_id));

            //Делаем словарь [ID_СКЛАДА] --> [product_id]=[quantity]
            Dictionary<int, Dictionary<int, int>> stockIdToPidQuantityMap = stockQuantityAll
                .Where(i => i.quantity.HasValue)
                .GroupBy(i => i.stock_partner_id)
                .ToDictionary(g => g.Key, g => g.ToDictionary(i => i.product_id, i => i.quantity.Value));

            //Получаем список ID всевозможных складов в товарах этого бренда
            int[] allStocksInProducts = stockIdToPidQuantityMap.Keys.ToArray();

            //Получаем список складов которых пользователь указал на странице /vse-instrumenti-export, если ничего не указал - берем все склады
            int[] validStockIds = string.IsNullOrWhiteSpace(data.checked_stocks) ?
                    Enumerable.Empty<int>().ToArray() :
                    data.checked_stocks.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => int.Parse(i))
                    .ToArray();

            //Проверяем, брать ли все склады, либо складывать остатки только у нужных складов
            bool addAllStocks =
                validStockIds.Count() == 0 ||
                allStocksInProducts.Except(validStockIds).Count() == 0;

            //Суммируем остатки, либо если пропускается - то берется просто поле quantity (сумма всех складов)
            if (!addAllStocks && validStockIds.Any())
            {
                SumQuantityFromCheckedStocks(products, stockIdToPidQuantityMap, validStockIds);
            }

            //Для дополнительных шаблонов, где кастомное заполненеие данных о цене
            if (shouldFilStockData)
            {
                foreach (var product in products)
                {
                    product.stock_data = stockQuantityAll.Where(p => p.product_id == product.product_id).ToArray();
                }
            }

            //Исключаем товары с 0 остатком
            products.RemoveAll(p => p.quantity == 0);

            //Исключаем все товары с нулевой ценой
            products.RemoveAll(p => p.price == decimal.Zero);

            //Исключаем все товары без EAN
            products.RemoveAll(p => p.ean is null || (p.ean?.Length ?? 0) != 13);

            return products;
        }

        private static void SumQuantityFromCheckedStocks(List<ProductEntity> products,
            Dictionary<int, Dictionary<int, int>> stockIdToPidQuantityMap,
            int[] validStockIds)
        {
            foreach (var product in products)
            {
                product.quantity = 0;

                foreach (var stockId in validStockIds)
                {
                    if (stockIdToPidQuantityMap.TryGetValue(stockId, out var dic))
                    {
                        if (dic.TryGetValue(product.product_id, out var quantity) && quantity > 0)
                        {
                            product.quantity += quantity;
                        }
                    }
                }
            }
        }
    }
}
