using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;
using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates
{
    public static class PrikatReportTemplateFactory
    {
        public static PrikatReportTemplateBase Create(string manufacturerName,
            CurrencyType currency,
            decimal discount,
            decimal currentCurrencyRate,
            VseInstrumentiReportOptions options)
        {
            if (manufacturerName.Equals("Pro'sKit"))
            {
                return new PrikatProskitReportTemplate(manufacturerName, currency, discount, currentCurrencyRate, options);
            }
            return new PrikatDefaultReportTemplate(manufacturerName, currency, discount, currentCurrencyRate, options);
        }
    }
}