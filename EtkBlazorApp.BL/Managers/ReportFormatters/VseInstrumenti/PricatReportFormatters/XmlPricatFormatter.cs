using EtkBlazorApp.DataAccess.Entity;
using System;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;

public sealed class XmlPricatFormatter : PricatFormatterBase
{
    public override void WriteProductEntry(ProductEntity product, decimal rrcPrice, decimal sellPrice)
    {
        throw new NotImplementedException();
    }
}
