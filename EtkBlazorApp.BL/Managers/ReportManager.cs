using EtkBlazorApp.DataAccess;

namespace EtkBlazorApp.BL
{
    public class ReportManager
    {        
        public PrikatReportFormatter Prikat { get; } 

        public ReportManager(ICurrencyChecker currencyCheker, ITemplateStorage templateStorage, IProductStorage productStorage)
        {
            Prikat = new PrikatReportFormatter(currencyCheker, templateStorage, productStorage);
        }
    }
}
