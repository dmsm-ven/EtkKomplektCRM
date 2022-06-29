using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates
{
    public class PrikatDefaultReportTemplate : PrikatReportTemplateBase
    {
        public PrikatDefaultReportTemplate(string manufacturer, CurrencyType currency) : base(manufacturer, currency) { }
    }
}
