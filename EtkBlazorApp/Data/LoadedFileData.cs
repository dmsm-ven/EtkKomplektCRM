using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Data
{
    public class LoadedFileData
    {
        public string FileName { get; set; }
        public string TemplateName { get; set; }
        public int RecordsInFile { get; set; }
        public string TempFilePath { get; set; }
    }
}
