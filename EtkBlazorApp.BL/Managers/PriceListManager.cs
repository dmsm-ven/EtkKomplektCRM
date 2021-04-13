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

        /// <summary>
        /// Считывает строки из прайс-листа и сохраняет их в списке. Этот метод специально для дальнейшнего ручного обновления товаров на сайте
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task UploadTemplate(Type templateType, Stream stream)
        {
            if (templateType == null) { throw new NotFoundedPriceListTemplateException(); }

            string fileName = Path.GetTempFileName();
            using (var fs = File.Create(fileName))
            {
                await stream.CopyToAsync(fs);
            }

            try
            {
                IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, fileName);
                var templateInfo = await GetTemplateDescription(templateType);

                var data = await templateInstance.ReadPriceLines(null);
                AddNewPriceLines(data);
                ApplyDiscounts(data, templateInfo.discount, templateInfo.nds);

                LoadedFiles.Add(new LoadedPriceListTemplateData(templateInstance, templateInfo, data));
            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(fileName);
            }       
        }

        /// <summary>
        /// Считывает строки из прайс-листа и возращает их напрямую, не добавляя в спиоск загруженных. Этот метод специально для автоматических задач
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<List<PriceLine>> ReadTemplateLines(Type templateType, Stream stream)
        {
            if (templateType == null) { throw new ArgumentNullException(nameof(templateType)); }

            List<PriceLine> list = new List<PriceLine>();

            string fileName = Path.GetTempFileName();

            using (var fs = File.Create(fileName))
            {
                await stream.CopyToAsync(fs);
            }

            //Скачали данные из потока и записали в временный файл
            if (!File.Exists(fileName)) { return list; }

            try
            {
                IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, fileName);
                var templateInfo = await GetTemplateDescription(templateType);

                list = await templateInstance.ReadPriceLines(null);
                ApplyDiscounts(list, templateInfo.discount, templateInfo.nds);

                return list;
            }
            catch
            {
                throw;
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
            if (discount == decimal.Zero && addNds == false) { return; }

            foreach (var line in list.Where(line => line.Price.HasValue))
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

        public List<PriceLine> PriceLinesOfManufacturer(string manufacturer)
        {
            return PriceLines.OrderBy(pl => pl.Manufacturer).Where(line => line.Manufacturer.Equals(manufacturer, StringComparison.OrdinalIgnoreCase)).ToList();
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

                    if(line.Quantity.HasValue && linkedLine.Quantity.HasValue)
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
    }
}
