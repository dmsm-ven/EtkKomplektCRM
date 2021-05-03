using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class CsvPriceListTemplateBase : PriceListTemplateReaderBase, IPriceListTemplate
    {
        public string FileName { get; }

        public CsvPriceListTemplateBase(string fileName) 
        { 
            FileName = fileName; 
        }

        protected async Task<List<string[]>> ReadCsvLines(char cellSeparator = '\t', Encoding encoding = null)
        {
            var lines = await Task.Run(() => File.ReadAllLines(FileName, encoding ?? Encoding.Default)
                  .Select(line => line.Split(cellSeparator))
                  .ToList());

            return lines;
        }

        public abstract Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null);  
    }
}
