using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.BL
{
    public class SymmetronPriceListTemplate : ExcelPriceListTemplateBase
    {
        public override List<PriceLine> ReadPriceLines(string fileName)
        {
            throw new NotImplementedException();
        }

        public override List<PriceLine> ReadPriceLines()
        {
            throw new NotImplementedException();
        }

    }

    public abstract class ExcelPriceListTemplateBase : IPriceListTemplate
    {
        public abstract List<PriceLine> ReadPriceLines(string fileName);
        public abstract List<PriceLine> ReadPriceLines();
    }

    public interface IPriceListTemplate
    {
        List<PriceLine> ReadPriceLines();
    }

    public class PriceLine
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public Currency Currency { get; set; }
    }
}
