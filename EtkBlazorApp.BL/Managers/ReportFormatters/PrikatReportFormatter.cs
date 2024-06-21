using EtkBlazorApp.BL.Templates.PrikatTemplates;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Entity.Marketplace;
using EtkBlazorApp.DataAccess.Repositories;
using Humanizer;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly PriceListManager priceListManager;

        public VseInstrumentiReportGenerator(ICurrencyChecker currencyChecker,
            IPrikatTemplateStorage templateStorage,
            IProductStorage productStorage,
            PriceListManager priceListManager)
        {
            this.currencyChecker = currencyChecker;
            this.templateStorage = templateStorage;
            this.productStorage = productStorage;
            this.priceListManager = priceListManager;
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

            var templateSource = await templateStorage.GetPrikatTemplates();

            string fileName = Path.GetTempPath() + $"prikat_{DateTime.Now.ToShortDateString().Replace(".", "_")}.csv";
            try
            {
                await FillExportFile(fileName, options, templateSource);

                nlog.Info("Выгрузка ВИ Pricat создана {fileName}. Длительность выполнения: {elapsed}", createdFileName, sw.Elapsed.Humanize());
            }
            catch (Exception ex)
            {
                nlog.Error("Ошибка создания выгрузки для ВсеИнструменты. Детали: {stack}", ex.StackTrace);
                throw;
            }

            return fileName;
        }

        private async Task FillExportFile(string fileName, VseInstrumentiReportOptions options, List<PrikatReportTemplateEntity> templateSource)
        {
            var fi = new FileInfo(fileName);
            nlog.Trace("Начало создания выгрузки ВИ (ПРИКАТ) с именем файла {fileName}", fileName);

            await Task.Run(async () =>
            {
                using (var fs = new FileStream(fileName, FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var data in templateSource)
                    {
                        await InsertTemplateInfo(sw, data, options);
                    }
                }
            });
        }

        private async Task InsertTemplateInfo(StreamWriter sw, PrikatReportTemplateEntity data, VseInstrumentiReportOptions options)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var currency = Enum.Parse<CurrencyType>(data.currency_code);
            decimal currentCurrencyRate = await currencyChecker.GetCurrencyRate(currency);

            var template = PrikatReportTemplateFactory.Create(data.manufacturer_name, currency);
            template.Discount1 = data.discount1;
            template.Discount2 = data.discount2;
            template.CurrencyRatio = currentCurrencyRate;
            template.GLN = options.GLN;

            List<ProductEntity> products = await PrepareProducts(data);

            template.AppendLines(products, sw);

            nlog.Trace("Выгрузка товаров для бренда {brandName} ({productsCount}). Длительность загрузки для этого бренда: {elapsed}",
                data.manufacturer_name, products.Count, stopwatch.Elapsed.Humanize());
        }

        private async Task<List<ProductEntity>> PrepareProducts(int manufacturer_id)
        {
            var products = await productStorage.ReadProducts(manufacturer_id);

            /*var uncheckedStocks = options.UsePartnerStock?.Where(c => !c.Value);
            if (uncheckedStocks != null && uncheckedStocks.Any())
            {
                foreach (var stockPartner in uncheckedStocks)
                {
                    var additionalStock = await productStorage.GetProductQuantityInAdditionalStock((int)stockPartner.Key);
                    products.ForEach(product => product.quantity -= additionalStock.FirstOrDefault(p => p.product_id == product.product_id)?.quantity ?? 0);
                }

                products.ForEach(product => product.quantity = Math.Max(product.quantity, 0));
            }
            */

            await ApplyCustomProductsHandler(products, manufacturer_id);

            return products;
        }

        private async Task ApplyCustomProductsHandler(List<ProductEntity> products, int manufacturer_id)
        {
            /*
            int PROSKIT_MANUFACTURER_ID = 1;
            //int MEAN_WELL_MANUFACTURER_ID = 37;
            //int MEAN_WELL_MINIMUM_QUANTITY = 40;

            //if (manufacturer_id == MEAN_WELL_MANUFACTURER_ID)
            //{
            //    products.RemoveAll(product => product.quantity < MEAN_WELL_MINIMUM_QUANTITY);
            //}

            if (manufacturer_id == PROSKIT_MANUFACTURER_ID)
            {
                var mainStock = await productStorage.GetProductQuantityInAdditionalStock((int)StockName._1C);
                var secondStock = await productStorage.GetProductQuantityInAdditionalStock((int)StockName.Symmetron);
                Dictionary<int, int> quantityInStocks = mainStock
                    .Concat(secondStock)
                    .GroupBy(p => p.product_id)
                    .ToDictionary(i => i.Key, j => j.Sum(p => p.quantity));

                products.ForEach(product =>
                {
                    product.quantity = quantityInStocks.ContainsKey(product.product_id) ? quantityInStocks[product.product_id] : 0;
                });
            }
            */
        }
    }
}
