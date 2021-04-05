using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public class LoadedPriceListTemplateData
    {
        public List<PriceLine> ReadedPriceLines { get; }
        public IPriceListTemplate TemplateInstance { get; }
        public PriceListTemplateEntity TemplateDescription { get; }

        public LoadedPriceListTemplateData(IPriceListTemplate template, PriceListTemplateEntity description, List<PriceLine> readedPriceLines)
        {
            TemplateInstance = template;
            TemplateDescription = description;
            ReadedPriceLines = readedPriceLines;
        }
    }
}
