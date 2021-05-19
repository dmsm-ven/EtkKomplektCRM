using EtkBlazorApp.DataAccess;

namespace EtkBlazorApp.BL
{
    public class ReportManager
    {        
        public VseInstrumentiReportGenerator Prikat { get; } 

        public ReportManager(ICurrencyChecker currencyCheker, 
            IPrikatTemplateStorage templateStorage,
            IProductStorage productStorage, 
            PriceListManager priceListManager)
        {
            Prikat = new VseInstrumentiReportGenerator(currencyCheker, templateStorage, productStorage, priceListManager);
        }
    }
}
