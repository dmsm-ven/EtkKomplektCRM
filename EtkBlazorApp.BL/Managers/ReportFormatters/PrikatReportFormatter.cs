using EtkBlazorApp.BL.Templates;
using EtkBlazorApp.BL.Templates.PrikatTemplates;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class PrikatReportGenerator
    {
        private readonly ICurrencyChecker currencyChecker;
        private readonly ITemplateStorage templateStorage;
        private readonly IProductStorage productStorage;
        private readonly PriceListManager priceListManager;

        public PrikatReportGenerator(ICurrencyChecker currencyChecker, 
            ITemplateStorage templateStorage, 
            IProductStorage productStorage, 
            PriceListManager priceListManager)
        {
            this.currencyChecker = currencyChecker;
            this.templateStorage = templateStorage;
            this.productStorage = productStorage;
            this.priceListManager = priceListManager;
        }

        public async Task<string> Create(bool inStock, bool hasEan)
        {
            var templateSource = (await templateStorage.GetPrikatTemplates()).Where(t => t.enabled);

            string fileName = Path.GetTempPath() + $"prikat_{DateTime.Now.ToShortDateString().Replace(".", "_")}.csv";
            await Task.Run(async () =>
            {
                using (var fs = new FileStream(fileName, FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var data in templateSource)
                    {
                        await InsertTemplateInfo(inStock, hasEan, sw, data);
                    }
                }
            });

            return fileName;
        }

        private async Task InsertTemplateInfo(bool inStock, bool hasEan, StreamWriter sw, PrikatReportTemplateEntity data)
        {
            var currency = Enum.Parse<CurrencyType>(data.currency_code);
            decimal currentCurrencyRate = await currencyChecker.GetCurrencyRate(currency);

            var template = new PrikatDefaultReportTemplate(data.manufacturer_name, currency)
            {
                Discount1 = data.discount1,
                Discount2 = data.discount2,
                CurrencyRatio = currentCurrencyRate,
                IsProductHasEan = hasEan,
                IsProductInStock = inStock
            };

            var products = await productStorage.ReadProducts(data.manufacturer_id);
            var priceLines = priceListManager.PriceLinesOfManufacturer(data.manufacturer_name);

            template.AppendLines(products, priceLines, sw);
        }
    }
}
