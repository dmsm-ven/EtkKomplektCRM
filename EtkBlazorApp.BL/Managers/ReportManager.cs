using EtkBlazorApp.DataAccess;

namespace EtkBlazorApp.BL
{
    public class ReportManager
    {        
        public PrikatReportGenerator Prikat { get; } 

        public ReportManager(ICurrencyChecker currencyCheker, 
            IPrikatTemplateStorage templateStorage,
            IProductStorage productStorage, 
            PriceListManager priceListManager)
        {
            Prikat = new PrikatReportGenerator(currencyCheker, templateStorage, productStorage, priceListManager);
        }
    }
}
