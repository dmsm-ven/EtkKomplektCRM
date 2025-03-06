using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;

public abstract class PricatFormatterBase
{
    protected static readonly char[] PRODUCT_NAME_INVALID_CHARS =
        ("#%&@*^$<>#!?()[]{}\t\r\n".ToList().Concat(Path.GetInvalidFileNameChars())).ToArray();

    protected static readonly decimal[] DEFAULT_DIMENSIONS = new decimal[] { 150, 100, 100, 0.4m };
    protected PrikatReportTemplateBase CurrentTemplate { get; private set; }
    protected StreamWriter StreamWriter { get; private set; }
    protected readonly IReadOnlyDictionary<string, string> UnitCodeDictionary = new Dictionary<string, string>()
    {
        ["грамм"] = "GRM",
        ["килограмм"] = "KGM",
        ["литр"] = "LTR",
        ["миллиметр"] = "MMT",
        ["квадратный метр"] = "MTK",
        ["кубический метр"] = "MTQ",
        ["метр"] = "MTR",
        ["миллиграмм"] = "MGM",
        ["миллилитр"] = "MLT",
        ["миллиметр кубический"] = "MMQ",
        ["штук"] = "PCE",
        ["дециметр кубический"] = "DMQ",
        ["сантиметр"] = "CMT",
        ["упаковка"] = "NMP",
        ["бобина(бухта)"] = "NBB",
    };

    /// <summary>
    /// Максимальная длинна 35 символов. Этот код должен был генерироватся автоматически в 1c при создании нового документа
    /// </summary>
    /// <returns></returns>
    public static string GenerateDocumentNumber(DateTime generationDateTime)
    {
        return generationDateTime.ToString("yyyyMMddHHmmss");
    }

    public abstract void WriteProductEntry(ProductEntity product, decimal rrcPrice, decimal sellPrice);

    public virtual void OnDocumentStart(DateTime generationDateTime) { }

    public virtual void OnDocumentEnd() { }

    public void SetStreamWriter(StreamWriter sw)
    {
        StreamWriter = sw;
    }

    public void SetCurrentTemplate(PrikatReportTemplateBase template)
    {
        CurrentTemplate = template;
    }

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
    protected string RemoveInvalidChars(string rawString)
    {
        if (rawString.Any(ch => PRODUCT_NAME_INVALID_CHARS.Contains(ch)))
        {
            string clearString = rawString;

            foreach (var invalidChar in PRODUCT_NAME_INVALID_CHARS)
            {
                if (clearString.Contains(invalidChar))
                {
                    clearString = clearString.Replace(invalidChar.ToString(), string.Empty);
                }
            }

            clearString = Regex.Replace(clearString, " {2, }", " ").Trim();

            return clearString;
        }

        return rawString;
    }
}
