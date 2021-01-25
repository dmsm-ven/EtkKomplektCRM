using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
{
    public class ReportManager
    {        
        public PrikatReportFormatter Prikat { get; } 
        public OzonReportFormatter Ozon { get; } 
        public WebsiteUpdatedDataFormatter WebsiteUpdatedData { get; } 

        public ReportManager()
        {
            Prikat = new PrikatReportFormatter();
            Ozon = new OzonReportFormatter();
            WebsiteUpdatedData = new WebsiteUpdatedDataFormatter();
        }
    }
}
