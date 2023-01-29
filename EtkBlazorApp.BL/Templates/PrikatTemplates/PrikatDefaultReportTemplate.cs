using EtkBlazorApp.Core.Data;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates
{
    public class PrikatDefaultReportTemplate : PrikatReportTemplateBase
    {
        public PrikatDefaultReportTemplate(string manufacturer, CurrencyType currency) : base(manufacturer, currency) { }
    }
}
