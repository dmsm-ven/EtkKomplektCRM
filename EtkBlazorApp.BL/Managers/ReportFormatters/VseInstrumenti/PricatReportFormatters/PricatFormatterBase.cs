using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;

public abstract class PricatFormatterBase
{
    protected static readonly char[] PRODUCT_NAME_INVALID_CHARS = "#%&@*^$<>#!?()[]{}\t\r\n".ToCharArray();
    protected static readonly decimal[] DEFAULT_DIMENSIONS = new decimal[] { 150, 100, 100, 0.4m };
    protected static readonly string LENGTH_UNIT = "миллиметр";
    protected static readonly string WEIGHT_UNIT = "килограмм";
    protected PrikatReportTemplateBase CurrentTemplate { get; private set; }
    protected StreamWriter StreamWriter { get; private set; }

    //TODO: тут проблема с SKU из-за изначально неверного понимания какой SKU должен быть
    //Должен быть уникальный SKU (вида: "ETK-123456") а сейчас тут, в том числе, артикулы от симметрона вида: "00123456"
    protected string GetSku(ProductEntity product)
    {
        string sku =
            !string.IsNullOrWhiteSpace(product.sku) ? product.sku :
            !string.IsNullOrWhiteSpace(product.model) ? product.model :
            $"ETK-{product.product_id}";

        return sku;
    }

    //Не использовать символы (),{},[] ЗНАКИ ТАБУЛЯЦИИ И ДРУГИЕ: #,%,&,@,*,^,$,< >,#,!,?) и длина строки желательно не больше 100 символов
    //(из спецификации Формат "CISLINK XML  File Client FTP WEBService_[Все инструменты]_[v.5.2.8].docx")
    protected string ClearProductName(string rawName)
    {
        if (rawName.Any(ch => PRODUCT_NAME_INVALID_CHARS.Contains(ch)))
        {
            string clearName = rawName;

            foreach (var invalidChar in PRODUCT_NAME_INVALID_CHARS)
            {
                if (clearName.Contains(invalidChar))
                {
                    clearName = clearName.Replace(invalidChar.ToString(), string.Empty);
                }
            }

            clearName = Regex.Replace(clearName, " {2, }", " ").Trim();

            return clearName;
        }

        return rawName;
    }

    public abstract void WriteProductEntry(ProductEntity product, decimal rrcPrice, decimal sellPrice);

    public virtual void OnDocumentStart()
    {

    }
    public virtual void OnDocumentEnd()
    {
        StreamWriter.Flush();
    }

    public void SetStreamWriter(StreamWriter sw)
    {
        StreamWriter = sw;
    }

    public void SetCurrentTemplate(PrikatReportTemplateBase template)
    {
        CurrentTemplate = template;
    }
}
