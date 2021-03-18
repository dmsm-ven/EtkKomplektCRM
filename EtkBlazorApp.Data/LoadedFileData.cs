namespace EtkBlazorApp.Data
{
    public class LoadedFileData
    {
        public string FileName { get; set; }
        public string TemplateName { get; set; }
        public int RecordsInFile { get; set; }
        public string TempFilePath { get; set; }

        public IPriceListTemplate Template { get; }

        public LoadedFileData(IPriceListTemplate template)
        {
            Template = template;
        }    
    }
}
