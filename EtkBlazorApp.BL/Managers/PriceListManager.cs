using EtkBlazorApp.BL.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
{
    public class PriceListManager
    {
        public List<PriceLine> PriceLines { get; }
        public List<LoadedFileData> LoadedFiles { get; }
        public IEnumerable<IPriceListTemplate> LoadedTemplates => LoadedFiles.Select(f => f.Template);

        private readonly IPriceLineLoadCorrelator correlator;

        public PriceListManager(IPriceLineLoadCorrelator correlator)
        {
            PriceLines = new List<PriceLine>();
            LoadedFiles = new List<LoadedFileData>();
            this.correlator = correlator;
        }

        public async Task<int> LoadPriceList(IPriceListTemplate template, CancellationToken? token)
        {
            if (template != null)
            {              
                var data = await Task.Run(() => template.ReadPriceLines(token));
                AddNewPriceLines(data);

                var fileData = new LoadedFileData(template)
                {
                    RecordsInFile = data.Count,
                    TemplateName = template.GetType().Name
                };

                LoadedFiles.Add(fileData);
                return data.Count;
            }

            return 0;
        }

        public void RemovePriceList(LoadedFileData data)
        {
            LoadedFiles.Remove(data);
            PriceLines.RemoveAll(line => line.Template == data.Template);
        }

        private void AddNewPriceLines(List<PriceLine> newLines)
        {
            if (PriceLines.Count == 0)
            {
                PriceLines.AddRange(newLines);
            }
            else
            {
                foreach (var line in newLines)
                {
                    var linkedLine = correlator.FindCorrelation(line, PriceLines);

                    if (linkedLine != null)
                    {
                        // если SpecialLine то прибавляем остаток на складе к тещей записи
                        // Пример - загрузка SymmetronPriceListTemplate и сразу за ним DipolQuantityTemplate или 1c остатки
                        // В итоге добавятся цена и остатки Symmetron, а так же приплюсуются остатки из Dipol

                        if (line.IsSpecialLine && (line.Quantity.HasValue && linkedLine.Quantity.HasValue))
                        {
                            linkedLine.Quantity += line.Quantity;
                        }
                        else
                        {
                            if (line.Price.HasValue)
                            {
                                linkedLine.Price = line.Price;
                            }
                            if (line.Quantity.HasValue)
                            {
                                linkedLine.Quantity = line.Quantity;
                            }
                        }
                        continue;
                    }

                    PriceLines.Add(line);
                }
            }
        }
    }
}
