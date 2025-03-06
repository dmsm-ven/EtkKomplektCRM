using EtkBlazorApp.DataAccess.Entity;
using System.Globalization;
using System.IO;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;

public sealed class CsvPricatFormatter : PricatFormatterBase
{
    public override void WriteProductEntry(ProductEntity product, decimal rrcPrice, decimal sellPrice)
    {
        string rrcPriceString = rrcPrice.ToString($"F{CurrentTemplate.Precission}", new CultureInfo("en-EN"));
        string sellPriceString = sellPrice.ToString($"F{CurrentTemplate.Precission}", new CultureInfo("en-EN"));

        string length = (product.length != decimal.Zero ? product.length : DEFAULT_DIMENSIONS[0]).ToString("F2", new CultureInfo("en-EN"));
        string width = (product.width != decimal.Zero ? product.width : DEFAULT_DIMENSIONS[1]).ToString("F2", new CultureInfo("en-EN"));
        string height = (product.height != decimal.Zero ? product.height : DEFAULT_DIMENSIONS[2]).ToString("F2", new CultureInfo("en-EN"));
        string weight = (product.weight != decimal.Zero ? product.weight : DEFAULT_DIMENSIONS[3]).ToString("F4", new CultureInfo("en-EN"));

        string productName = ClearProductName(product.name);
        string sku = GetSku(product);

        WriteCell(CurrentTemplate.Options.GLN_ETK);   //GLN поставщика
        WriteCell(); //Позиция
        WriteCell(product.ean); //Штрихкод
        WriteCell(); //Артикул товара покупателя
        WriteCell(sku); //Артикул товара поставщика 
        WriteCell(productName); //Наименование
        WriteCell("1"); //Кол-во
        WriteCell("шт."); //Единицы измерения
        WriteCell(); //Товарная группа
        WriteCell(CurrentTemplate.Manufacturer); //Бренд
        WriteCell(); //Суббренд
        WriteCell(); //Вариант названия продукта
        WriteCell(); //Функциональное название
        WriteCell(length); //Глубина
        WriteCell(LENGTH_UNIT); //Единицы измерения
        WriteCell(width); //Ширина
        WriteCell(LENGTH_UNIT); //Единицы измерения
        WriteCell(height); //Высота
        WriteCell(LENGTH_UNIT); //Единицы измерения
        WriteCell(); //Объем
        WriteCell(); //Единицы измерения
        WriteCell(weight); //Вес, брутто
        WriteCell(WEIGHT_UNIT); //Единицы измерения
        WriteCell(); //Страна производитель
        WriteCell(); //Годен до
        WriteCell(rrcPriceString); //Рекомендованная цена
        WriteCell(sellPriceString); //Закупочная цена
        WriteCell(product.quantity.ToString()); //Количество остатков на скаладе
        WriteCell(CurrentTemplate.Currency.ToString().ToLower()); //Рекомендованная валюта
        WriteCell(CurrentTemplate.Currency.ToString().ToLower()); //Закупчная валюта
        StreamWriter.WriteLine();
    }

    private void WriteCell(string value = null)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            StreamWriter.Write(value.Replace(";", " ")?.Trim());
        }
        StreamWriter.Write(";");
    }
}
