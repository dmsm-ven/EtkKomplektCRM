using System;
using System.Collections.Generic;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates;

[PriceListTemplateGuid("9ED3EC07-268A-48C3-BE00-771BEC13A3CF")]
public class TdEskoPriceListTemplate : ExcelPriceListTemplateBase
{
    private readonly string[] ValidBrands = new[] { "Fluke", "Testo", "АКИП" };

    public TdEskoPriceListTemplate(string fileName) : base(fileName) { }

    protected override List<PriceLine> ReadDataFromExcel()
    {
        var list = new List<PriceLine>();

        for (int row = 2; row < tab.Dimension.Rows; row++)
        {
            string name = tab.GetValue<string>(row, 2);

            string manufacturer = GetManufacturerFromName(name);
            if (string.IsNullOrWhiteSpace(manufacturer) || SkipThisBrand(manufacturer)) { continue; }

            string sku = tab.GetValue<string>(row, 3);

            var quantity = ParseQuantity(tab.GetValue<string>(row, 4));

            var priceLine = new PriceLine(this)
            {
                Name = name,
                Manufacturer = manufacturer,
                Model = sku,
                Sku = sku,
                Quantity = quantity
            };

            list.Add(priceLine);
        }


        return list;
    }

    private string GetManufacturerFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        foreach (var brand in ValidBrands)
        {
            if (name.Contains(brand, StringComparison.OrdinalIgnoreCase))
            {
                return brand;
            }
        }

        return string.Empty;
    }
}