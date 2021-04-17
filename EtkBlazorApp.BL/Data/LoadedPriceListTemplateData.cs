using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public class LoadedPriceListTemplateData
    {
        public List<PriceLine> ReadedPriceLines { get; }
        public IPriceListTemplate TemplateInstance { get; }
        public PriceListTemplateEntity TemplateDescription { get; }
        public DateTime DateAdded { get; } = DateTime.Now;

        public LoadedPriceListTemplateData(IPriceListTemplate template, PriceListTemplateEntity description, List<PriceLine> readedPriceLines)
        {
            TemplateInstance = template;
            TemplateDescription = description;
            ReadedPriceLines = readedPriceLines;
        }
    }
}
