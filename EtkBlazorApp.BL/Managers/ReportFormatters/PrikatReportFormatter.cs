using EtkBlazorApp.BL.Templates;
using EtkBlazorApp.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class PrikatReportFormatter
    {
        private readonly ICurrencyChecker currencyChecker;
        private readonly ITemplateStorage templateStorage;
        private readonly IProductStorage productStorage;

        public PrikatReportFormatter(ICurrencyChecker currencyChecker, ITemplateStorage templateStorage, IProductStorage productStorage)
        {
            this.currencyChecker = currencyChecker;
            this.templateStorage = templateStorage;
            this.productStorage = productStorage;
        }

        public async Task Create(string outputFileName, bool removeEmptyStock)
        {
            await Task.Run(async() =>
            {
                using (var sw = new StreamWriter(File.Open(outputFileName, FileMode.Create), new UTF8Encoding()))
                {
                    var templateSource = (await templateStorage.GetPrikatTemplates()).Where(t => t.enabled);
                    foreach (var data in templateSource)
                    {
                        CurrencyType currency = (CurrencyType)Enum.Parse(typeof(CurrencyType), data.currency_code);
                        decimal currentCurrencyRate = await currencyChecker.GetCurrencyRate(currency);

                        var template = new PrikatReportTemplate(data.manufacturer_name, currency)
                        {
                            Discount1 = data.discount1,
                            Discount2 = data.discount2,
                            CurrencyRatio = currentCurrencyRate
                        };

                        var products = await productStorage.ReadProducts(data.manufacturer_id);

                        template.AppendLines(products, new List<PriceLine>(), removeEmptyStock, sw);
                    }
                }
            });
        }
        

    }
}
