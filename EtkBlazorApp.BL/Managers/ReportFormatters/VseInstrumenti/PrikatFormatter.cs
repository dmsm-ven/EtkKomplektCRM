using System;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti;

public sealed class XmlPricatFormatter : PricatFormatter
{

}
public sealed class CsvPricatFormatter : PricatFormatter
{

}

public interface PricatFormatter
{

}

public static class PricatFormatterFactory
{
    public static PricatFormatter Create(PricatFormat format)
    {
        PricatFormatter formatter = format switch
        {
            PricatFormat.Csv => new CsvPricatFormatter(),
            PricatFormat.Xml => new XmlPricatFormatter(),
            _ => throw new NotImplementedException($"Шаблон формата {Enum.GetName(format)} не реализован")
        };

        return formatter;
    }
}