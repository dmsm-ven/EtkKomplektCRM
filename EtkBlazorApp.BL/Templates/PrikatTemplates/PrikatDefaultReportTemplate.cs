using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;
using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates
{
    public class PrikatDefaultReportTemplate : PrikatReportTemplateBase
    {
        public PrikatDefaultReportTemplate(string manufacturer, CurrencyType currency, PricatFormatterBase formatter)
            : base(manufacturer, currency, formatter) { }
    }

    public static class PrikatReportTemplateFactory
    {
        public static PrikatReportTemplateBase Create(string manufacturerName, CurrencyType currency, PricatFormatterBase formatter)
        {
            if (manufacturerName.Equals("Pro'sKit"))
            {
                return new PrikatProskitReportTemplate(manufacturerName, currency, formatter);
            }
            return new PrikatDefaultReportTemplate(manufacturerName, currency, formatter);
        }
    }
}