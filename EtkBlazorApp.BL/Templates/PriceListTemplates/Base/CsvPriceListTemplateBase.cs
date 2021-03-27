using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class CsvPriceListTemplateBase : IPriceListTemplate
    {
        public string FileName { get; }

        public CsvPriceListTemplateBase(string fileName) 
        { 
            FileName = fileName; 
        }

        protected async Task<List<string[]>> ReadCsvLines()
        {
            var lines = await Task.Run(() => File.ReadAllLines(FileName, System.Text.Encoding.Default)
                  .Select(line => line.Split('\t'))
                  .ToList());

            return lines;
        }

        public abstract Task<List<PriceLine>> ReadPriceLines(CancellationToken? token = null);
    }
}
