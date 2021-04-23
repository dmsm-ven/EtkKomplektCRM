using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class PriceListManager
    {
        public event Action OnPriceListLoaded;

        public List<PriceLine> PriceLines { get; }
        public List<LoadedPriceListTemplateData> LoadedFiles { get; }
        public decimal NDS { get; } = 1.2m;

        private readonly IPriceLineLoadCorrelator correlator;
        private readonly ITemplateStorage templateStorage;

        public PriceListManager(IPriceLineLoadCorrelator correlator, ITemplateStorage templateStorage)
        {
            PriceLines = new List<PriceLine>();
            LoadedFiles = new List<LoadedPriceListTemplateData>();
            this.correlator = correlator;
            this.templateStorage = templateStorage;
        }

        public async Task UploadTemplate(Type templateType, Stream stream, string fileName)
        {
            var data = await ReadTemplateLines(templateType, stream, fileName, addFileData: true);
            AddNewPriceLines(data);
        }

        public async Task<List<PriceLine>> ReadTemplateLines(Type templateType, Stream stream, string fileName = null, bool addFileData = false)
        {
            if (templateType == null) { throw new ArgumentNullException(nameof(templateType)); }

            List<PriceLine> list = new List<PriceLine>();

            string filePath = Path.GetTempFileName();
            if (fileName != null)
            {
                filePath = filePath.Replace(".tmp", Path.GetExtension(fileName));
            }

            using (var fs = File.Create(filePath))
            {
                await stream.CopyToAsync(fs);
            }

            if (!File.Exists(filePath)) { return list; }

            try
            {
                IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, filePath);
                var templateInfo = await GetTemplateDescription(templateType);

                list = await templateInstance.ReadPriceLines(null);
                ApplyDiscounts(list, templateInfo.discount, templateInfo.nds);

                if (addFileData)
                {
                    var loadedFileInfo = new LoadedPriceListTemplateData(templateInstance, templateInfo, list, fileName);
                    LoadedFiles.Add(loadedFileInfo);
                }

                return list;
            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Применить наценки/скидки/НДС к загруженным ценам
        /// </summary>
        /// <param name="list"></param>
        /// <param name="discount"></param>
        /// <param name="addNds"></param>
        private void ApplyDiscounts(List<PriceLine> list, decimal discount, bool addNds)
        {
            var source = list.Where(line => line.Price.HasValue);
            bool emptyDiscount = (discount == decimal.Zero && addNds == false);

            if (emptyDiscount || source.Count() == 0) { return; }

            foreach (var line in source)
            {
                if (addNds)
                {
                    line.Price = AddNds(line.Price.Value, line.Currency);
                }
                if (discount != decimal.Zero)
                {
                    line.Price = Math.Round(line.Price.Value * (1m + (discount / 100m)), line.Currency == CurrencyType.RUB ? 0 : 2);
                }

            }
        }

        public void RemovePriceListAll()
        {
            LoadedFiles.Clear();
            PriceLines.Clear();
        }

        public void RemovePriceList(LoadedPriceListTemplateData data)
        {
            LoadedFiles.Remove(data);
            PriceLines.RemoveAll(line => line.Template == data.TemplateInstance);
        }

        private decimal AddNds(decimal price, CurrencyType currencyType)
        {
            if (currencyType == CurrencyType.RUB)
            {
                return Math.Floor(price * NDS);
            }
            else
            {
                return Math.Round(price * NDS, 4);
            }
        }

        private async Task<PriceListTemplateEntity> GetTemplateDescription(Type templateType)
        {
            var guid = ((PriceListTemplateGuidAttribute)templateType
                .GetCustomAttributes(typeof(PriceListTemplateGuidAttribute), false)
                .FirstOrDefault()).Guid;
            var info = (await templateStorage.GetPriceListTemplateById(guid));
            return info;
        }

        private void AddNewPriceLines(List<PriceLine> newLines)
        {
            if (PriceLines.Count == 0)
            {
                PriceLines.AddRange(newLines);
                return;
            }

            foreach (var line in newLines)
            {
                var linkedLine = correlator.FindCorrelation(line, PriceLines);

                if (linkedLine != null)
                {
                    if (line.Price.HasValue)
                    {
                        linkedLine.Price = line.Price;
                    }

                    if (line.Quantity.HasValue && linkedLine.Quantity.HasValue)
                    {
                        linkedLine.Quantity += line.Quantity;
                    }
                    else
                    {
                        linkedLine.Quantity = line.Quantity;
                    }
                    continue;
                }

                PriceLines.Add(line);
            }
        }

        public static string GetPriceListGuidByType(Type priceListTemplateType)
        {
            var id = ((PriceListTemplateGuidAttribute)priceListTemplateType
                .GetCustomAttributes(typeof(PriceListTemplateGuidAttribute), false)
                .FirstOrDefault())
                .Guid;

            return id;
        }
    }
}
