using EtkBlazorApp.BL.Interfaces;
using EtkBlazorApp.DataAccess;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
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
