using EtkBlazorApp.BL.Managers.ReportFormatters;

namespace EtkBlazorApp.BL.Managers
{
    public class ReportManager
    {
        public VseInstrumentiReportGenerator Prikat { get; private set; }
        public EtkKomplektReportGenerator EtkPricelist { get; private set; }

        public ReportManager(VseInstrumentiReportGenerator viGenerator, EtkKomplektReportGenerator etkExportGenerator)
        {
            Prikat = viGenerator;
            EtkPricelist = etkExportGenerator;
        }
    }
}
