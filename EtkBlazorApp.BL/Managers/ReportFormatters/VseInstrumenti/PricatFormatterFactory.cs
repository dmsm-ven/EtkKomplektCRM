using EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;
using System;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;

public static class PricatFormatterFactory
{
    public static PricatFormatterBase Create(VseInstrumentiReportOptions options)
    {
        PricatFormatterBase formatter = options.PricatFormat switch
        {
            PricatFormat.Csv => new CsvPricatFormatter(),
            PricatFormat.Xml => new XmlPricatFormatter(),
            _ => throw new NotImplementedException($"Шаблон формата {Enum.GetName(options.PricatFormat)} не реализован")
        };

        return formatter;
    }
}