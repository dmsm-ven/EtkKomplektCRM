using EtkBlazorApp.DataAccess;

namespace EtkBlazorApp.BL
{
    public class ReportManager
    {        
        public VseInstrumentiReportGenerator Prikat { get; } 
        public EtkKomplektReportGenerator EtkPricelist { get; }

        public ReportManager(ICurrencyChecker currencyCheker, 
            IPrikatTemplateStorage templateStorage,
            IProductStorage productStorage, 
            PriceListManager priceListManager)
        {
            Prikat = new VseInstrumentiReportGenerator(currencyCheker, templateStorage, productStorage, priceListManager);
            EtkPricelist = new EtkKomplektReportGenerator(productStorage);
        }
    }
}
