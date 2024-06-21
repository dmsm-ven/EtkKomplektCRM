using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates
{
    public class PrikatDefaultReportTemplate : PrikatReportTemplateBase
    {
        public PrikatDefaultReportTemplate(string manufacturer, CurrencyType currency) : base(manufacturer, currency) { }
    }

    public static class PrikatReportTemplateFactory
    {
        public static PrikatReportTemplateBase Create(string manufacturerName, CurrencyType currency)
        {
            if (manufacturerName.Equals("Pro'sKit"))
            {
                return new PrikatProskitReportTemplate(manufacturerName, currency);
            }
            return new PrikatDefaultReportTemplate(manufacturerName, currency);
        }
    }
}