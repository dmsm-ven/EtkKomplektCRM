using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Text;
using System.Xml;

namespace EtkBlazorApp.BL.Managers.ReportFormatters.VseInstrumenti.PricatReportFormatters;

public sealed class XmlPricatFormatter : PricatFormatterBase
{
    private readonly StringBuilder sb = new();
    private XmlWriter xml = null;

    public override void OnDocumentStart()
    {
        xml = XmlWriter.Create(sb, new XmlWriterSettings() { Indent = true });

        xml.WriteStartElement("Document");
        xml.WriteElementString("DocType", "PRICAT");
        xml.WriteElementString("SenderGLN", this.CurrentTemplate.Options.GLN_ETK);
        xml.WriteElementString("ReceiverGLN", this.CurrentTemplate.Options.GLN_VI);
    }

    public override void OnDocumentEnd()
    {
        xml.WriteEndElement(); // Document

        xml.Flush();
        StreamWriter.Write(sb.ToString());
        base.OnDocumentEnd();
    }
    public override void WriteProductEntry(ProductEntity product, decimal rrcPrice, decimal sellPrice)
    {
        throw new NotImplementedException();
    }
}
