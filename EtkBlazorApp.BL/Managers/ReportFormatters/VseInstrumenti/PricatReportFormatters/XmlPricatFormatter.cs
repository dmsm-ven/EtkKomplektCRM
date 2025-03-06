﻿using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;

public sealed class XmlPricatFormatter : PricatFormatterBase
{
    private readonly StringBuilder sb = new();
    private readonly XmlWriter xml = null;
    private readonly NumberFormatInfo numberFormat;

    public XmlPricatFormatter()
    {
        numberFormat = new NumberFormatInfo();
        numberFormat.NumberDecimalDigits = 4;
        numberFormat.NumberDecimalSeparator = ".";

        xml = XmlWriter.Create(sb, new XmlWriterSettings() { Indent = true });
    }

    public override void OnDocumentStart(DateTime generationDateTime)
    {
        xml.WriteStartElement("Document");

        xml.WriteElementString("DocType", "PRICAT");
        xml.WriteElementString("SenderGLN", this.CurrentTemplate.Options.GLN_ETK);
        xml.WriteElementString("ReceiverGLN", this.CurrentTemplate.Options.GLN_VI);
        xml.WriteElementString("Currency", "RUB");
        xml.WriteElementString("DocumentNumber", PricatFormatterBase.GenerateDocumentNumber(generationDateTime));
        xml.WriteElementString("DocumentDate", generationDateTime.ToString("yyyyMMdd"));
    }

    public override void WriteProductEntry(ProductEntity product, decimal rrcPrice, decimal sellPrice)
    {
        // DocDetail Начало нового элемента товара
        xml.WriteStartElement("DocDetail");

        // EAN товара – только цифры (max 14)
        xml.WriteElementString("EAN", product.ean);

        //Код товара в УС отправителя (max 35)
        xml.WriteElementString("SenderPrdCode", this.GetSku(product));

        // Название товара (max 170, желательно не более 100)
        xml.WriteElementString("ProductName", this.ClearProductName(product.name));

        //Единица измерения
        xml.WriteElementString("UOM", UnitCodeDictionary["штук"]);

        //Количество штук в упаковке (decimal 10.4)
        xml.WriteElementString("ItemsPerUnit", 1.ToString(numberFormat));

        //Количество  (decimal 10.4)
        xml.WriteElementString("QTY", product.quantity.ToString(numberFormat));

        //Закупочная цена (с ндс) КОНЕЧНАЯ ЗАКУПОЧНАЯ ЦЕНА С УЧЕТОМ ВСЕХ СКИДОК.
        xml.WriteElementString("Price2", sellPrice.ToString(numberFormat));

        //Заполняем дополнительные данные по товару
        WriteProductDetailOptions(product, rrcPrice);

        // DocDetail Конец нового элемента товара
        xml.WriteEndElement();
    }

    private void WriteProductDetailOptions(ProductEntity product, decimal rrcPrice)
    {
        //DocDetailOptions - START
        xml.WriteStartElement("DocDetailOptions");

        //Производитель MAX 100 символов
        AppendDocOption("Brand", this.CurrentTemplate.Manufacturer);

        //Длина max (15 символов)
        string length = (product.length != decimal.Zero ? product.length : DEFAULT_DIMENSIONS[0]).ToString(numberFormat);
        AppendDocOption("Depth", length);
        AppendDocOption("DepthUnit", UnitCodeDictionary["миллиметр"]);

        //Ширина max (15 символов)
        string width = (product.width != decimal.Zero ? product.width : DEFAULT_DIMENSIONS[1]).ToString(numberFormat);
        AppendDocOption("Width", width);
        AppendDocOption("WidthUnit", UnitCodeDictionary["миллиметр"]);

        //Высота max (15 символов)
        string height = (product.height != decimal.Zero ? product.height : DEFAULT_DIMENSIONS[2]).ToString(numberFormat);
        AppendDocOption("Height", height);
        AppendDocOption("HeightUnit", UnitCodeDictionary["миллиметр"]);

        //Закупочная валюта (обязательно указывать строчными («маленькими») буквами) 3 символа
        AppendDocOption("Currency", this.CurrentTemplate.Currency.ToString().ToLower());

        //Вес (15 символов)
        string weight = (product.weight != decimal.Zero ? product.weight : DEFAULT_DIMENSIONS[3]).ToString(numberFormat);
        AppendDocOption("Weight", height);
        AppendDocOption("WeightUnit", UnitCodeDictionary["килограмм"]);

        //Рекомендованная цена
        AppendDocOption("RetailPrice", rrcPrice.ToString(numberFormat));
        //Рекомендованная валюта (обязательно указывать строчными («маленькими») буквами)
        AppendDocOption("RetailCurrency", this.CurrentTemplate.Currency.ToString().ToLower());

        //DocDetailOptions - END
        xml.WriteEndElement();
    }

    private void AppendDocOption(string name, string value)
    {
        xml.WriteStartElement("DocOption");
        xml.WriteElementString("Name", name);
        xml.WriteElementString("Value", value);
        xml.WriteEndElement();
    }

    public override void OnDocumentEnd()
    {
        // Document END
        xml.WriteEndElement();

        xml.Flush();
        StreamWriter.Write(sb.ToString());
        base.OnDocumentEnd();
    }
}
