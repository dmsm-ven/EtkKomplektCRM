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

        WriteCell(StreamWriter, CurrentTemplate.GLN);   //GLN поставщика
        WriteCell(StreamWriter); //Позиция
        WriteCell(StreamWriter, product.ean); //Штрихкод
        WriteCell(StreamWriter); //Артикул товара покупателя
        WriteCell(StreamWriter, sku); //Артикул товара поставщика 
        WriteCell(StreamWriter, productName); //Наименование
        WriteCell(StreamWriter, "1"); //Кол-во
        WriteCell(StreamWriter, "шт."); //Единицы измерения
        WriteCell(StreamWriter); //Товарная группа
        WriteCell(StreamWriter, CurrentTemplate.Manufacturer); //Бренд
        WriteCell(StreamWriter); //Суббренд
        WriteCell(StreamWriter); //Вариант названия продукта
        WriteCell(StreamWriter); //Функциональное название
        WriteCell(StreamWriter, length); //Глубина
        WriteCell(StreamWriter, LENGTH_UNIT); //Единицы измерения
        WriteCell(StreamWriter, width); //Ширина
        WriteCell(StreamWriter, LENGTH_UNIT); //Единицы измерения
        WriteCell(StreamWriter, height); //Высота
        WriteCell(StreamWriter, LENGTH_UNIT); //Единицы измерения
        WriteCell(StreamWriter); //Объем
        WriteCell(StreamWriter); //Единицы измерения
        WriteCell(StreamWriter, weight); //Вес, брутто
        WriteCell(StreamWriter, WEIGHT_UNIT); //Единицы измерения
        WriteCell(StreamWriter); //Страна производитель
        WriteCell(StreamWriter); //Годен до
        WriteCell(StreamWriter, rrcPriceString); //Рекомендованная цена
        WriteCell(StreamWriter, sellPriceString); //Закупочная цена
        WriteCell(StreamWriter, product.quantity.ToString()); //Количество остатков на скаладе
        WriteCell(StreamWriter, CurrentTemplate.Currency.ToString().ToLower()); //Рекомендованная валюта
        WriteCell(StreamWriter, CurrentTemplate.Currency.ToString().ToLower()); //Закупчная валюта
        StreamWriter.WriteLine();
    }

    private void WriteCell(StreamWriter sw, string value = null)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            sw.Write(value.Replace(";", " ")?.Trim());
        }
        sw.Write(";");
    }
}
