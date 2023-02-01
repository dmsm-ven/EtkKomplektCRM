using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public decimal NDS { get; private set; }
        private readonly IPriceLineLoadCorrelator correlator;
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly ISettingStorage settings;

        public PriceListManager(IPriceLineLoadCorrelator correlator, IPriceListTemplateStorage templateStorage, ISettingStorage settings)
        {
            PriceLines = new List<PriceLine>();
            LoadedFiles = new List<LoadedPriceListTemplateData>();
            this.correlator = correlator;
            this.templateStorage = templateStorage;
            this.settings = settings;
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

        public async Task UploadTemplate(Type templateType, Stream stream, string fileName)
        {
            var data = await ReadTemplateLines(templateType, stream, fileName, addFileData: true);
            AddNewPriceLines(data);
        }

        /// <summary>
        /// Главный корневой метод считывающий прайс-лист, запускается и из переодических задач и при загрузке в ручную
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="addFileData"></param>
        /// <returns></returns>
        public async Task<List<PriceLine>> ReadTemplateLines(Type templateType, Stream stream, string fileName, bool addFileData = false)
        {
            if (templateType == null) { throw new ArgumentNullException(nameof(templateType)); }

            string filePath = Path.Combine(Path.GetTempPath(), fileName);
            using (var fs = File.Create(filePath))
            {
                await stream.CopyToAsync(fs);
            }

            try
            {
                IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, filePath);
                PriceListTemplateEntity templateInfo = await GetTemplateDescription(templateType);
                if (templateInstance is PriceListTemplateReaderBase pb)
                {
                    //Обязательно должно быть перед считываением, т.к. тут заполняются словари с преобразованиями
                    pb.FillTemplateInfo(templateInfo);
                }

                var list = await templateInstance.ReadPriceLines(null);

                FillLinkedStock(list, templateInfo.stock_partner_id);

                await ApplyDiscounts(list, templateInfo);

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

        private void FillLinkedStock(List<PriceLine> list, int? stock_partner_id)
        {
            if (!stock_partner_id.HasValue) { return; }

            StockName stock = (StockName)stock_partner_id.Value;
            foreach (var line in list.Where(l => l.Stock == StockName.None))
            {
                line.Stock = stock;
            }
        }

        /// <summary>
        /// Применить наценки/скидки/НДС к загруженным ценам
        /// </summary>
        /// <param name="list"></param>
        /// <param name="discount"></param>
        /// <param name="addNds"></param>
        private async Task ApplyDiscounts(IEnumerable<PriceLine> list, PriceListTemplateEntity templateInfo)
        {
            var source = list.Where(line => line.Price.HasValue);

            bool emptyDiscountGeneralDiscount = templateInfo.discount == decimal.Zero;
            bool emptyDiscountMap = templateInfo.manufacturer_discount_map.Count == 0;

            // берем цену из прайс-листа как есть. Т.к. нет ни наценки, ни НДС
            if (emptyDiscountGeneralDiscount && emptyDiscountMap && !templateInfo.nds)
            {
                return;
            }

            // словарь Бренд/Скидка
            Dictionary<string, decimal> discountsByBrand = templateInfo
                .manufacturer_discount_map
                .ToDictionary(i => i.name, i => i.discount);

            //Загружаем НДС
            NDS = (100m + (await settings.GetValue<int>("nds"))) / 100;

            foreach (var line in source)
            {
                //Сначала добавляет к цене NDS, если нужно
                if (templateInfo.nds)
                {
                    line.Price = AddNds(line.Price.Value, line.Currency);
                }

                decimal discountValue = emptyDiscountGeneralDiscount ? 0m : templateInfo.discount;
                // Если бренд есть в словаре, то берем скидку для него в первую очередь (заменяя стандартную от прайс-листа)
                if (!emptyDiscountMap && discountsByBrand.ContainsKey(line.Manufacturer))
                {
                    discountValue = discountsByBrand[line.Manufacturer];
                }

                //Высчитываем множитель
                decimal discountRatio = 1m + (discountValue / 100m);

                if (discountRatio != 1)
                {
                    line.Price = Math.Round(line.Price.Value * discountRatio, line.Currency == CurrencyType.RUB ? 0 : 2);
                }
            }
        }

        /// <summary>
        /// Прибавить НДС к загруженному прайс-листу
        /// </summary>
        /// <param name="price"></param>
        /// <param name="currencyType"></param>
        /// <returns></returns>
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
            var guid = templateType.GetPriceListGuidByType();
            var info = await templateStorage.GetPriceListTemplateById(guid);
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
    }
}
