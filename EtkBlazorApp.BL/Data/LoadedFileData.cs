using System.Linq;

namespace EtkBlazorApp.BL
{
    public class LoadedFileData
    {
        public string FileName => Template.FileName ?? string.Empty;
        public int RecordsInFile { get; set; }
        public string TemplateTitle { get; set; }

        public IPriceListTemplate Template { get; }

        public LoadedFileData(IPriceListTemplate template)
        {
            Template = template;
        }
    }
}
