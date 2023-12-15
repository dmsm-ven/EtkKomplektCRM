using EtkBlazorApp.DataAccess.Entity.PriceList;
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
        public DateTime DateAdded { get; } 
        public string FileName { get; } 

        public LoadedPriceListTemplateData(IPriceListTemplate template, PriceListTemplateEntity description, List<PriceLine> readedPriceLines, string fileName)
        {
            TemplateInstance = template;
            TemplateDescription = description;
            ReadedPriceLines = readedPriceLines;
            FileName = fileName;
            DateAdded = DateTime.Now;
        }
    }
}
