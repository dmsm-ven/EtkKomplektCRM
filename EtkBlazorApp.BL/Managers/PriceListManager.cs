using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Templates.PriceListTemplates.Base;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity.PriceList;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
{
    //TODO: разбить/упростить класс
    public class PriceListManager
    {
        public event Action OnPriceListLoaded;
        public decimal NDS { get; private set; }
        public List<PriceLine> PriceLines { get; }
        public List<LoadedPriceListTemplateData> LoadedFiles { get; }

        private readonly IPriceLineLoadCorrelator correlator;
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly ISettingStorageReader settings;
        private readonly ILogger<PriceListManager> logger;

        public PriceListManager(IPriceLineLoadCorrelator correlator,
            IPriceListTemplateStorage templateStorage,
            ISettingStorageReader settings,
            ILogger<PriceListManager> logger)
        {
            PriceLines = new List<PriceLine>();
            LoadedFiles = new List<LoadedPriceListTemplateData>();
            this.correlator = correlator;
            this.templateStorage = templateStorage;
            this.settings = settings;
            this.logger = logger;
        }

        /// <summary>
        /// Метод для Blazor вебинтерфейса личного кабинета
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
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
            if (templateType == null)
            {
                throw new ArgumentNullException(nameof(templateType));
            }

            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
            List<PriceLine> list = new();

            try
            {
                list = await ReadPriceLinesAndApplyModifiers(templateType, stream, fileName, addFileData, tempFilePath);
            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(tempFilePath);
            }

            return list;
        }

        /// <summary>
        /// Загрузить строки указанного шаблона и применить все модификаторы
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="addFileData"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<List<PriceLine>> ReadPriceLinesAndApplyModifiers(Type templateType, Stream stream, string fileName, bool addFileData, string filePath)
        {
            using (var fs = File.Create(filePath))
            {
                await stream.CopyToAsync(fs);
            }

            IPriceListTemplate templateInstance = (IPriceListTemplate)Activator.CreateInstance(templateType, filePath);
            PriceListTemplateEntity templateInfo = await GetTemplateDescription(templateType);
            if (templateInstance is PriceListTemplateReaderBase pb)
            {
                //Обязательно должно быть перед считываением, т.к. тут заполняются словари с преобразованиями
                pb.FillTemplateInfo(templateInfo);
            }

            var list = await templateInstance.ReadPriceLines(CancellationToken.None);

            await ApplyModifiers(list, templateInfo);

            if (addFileData)
            {
                var loadedFileInfo = new LoadedPriceListTemplateData(templateInstance, templateInfo, list, fileName);
                LoadedFiles.Add(loadedFileInfo);
            }

            return list;
        }

        /// <summary>
        /// Применить ПОСТ-подификаторы к считанным строкам прайс-листа
        /// </summary>
        /// <param name="templateInfo"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private async Task ApplyModifiers(List<PriceLine> list, PriceListTemplateEntity templateInfo)
        {
            SkipUnnecessaryManufacturers(list, templateInfo);

            FillLinkedStock(list, templateInfo.stock_partner_id);

            ApplyModelMap(list, templateInfo);

            await ApplyDiscounts(list, templateInfo);
        }

        /// <summary>
        /// Применяем черный/белый список для производителей в прайс-листе
        /// </summary>
        /// <param name="templateInfo"></param>
        /// <param name="list"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SkipUnnecessaryManufacturers(List<PriceLine> list, PriceListTemplateEntity templateInfo)
        {
            if ((templateInfo?.manufacturer_skip_list?.Count ?? 0) == 0)
            {
                return;
            }

            //Применяем черный лист
            var blackList = templateInfo.manufacturer_skip_list.Where(i => i.list_type == "black_list").ToArray();
            if (blackList.Length > 0)
            {
                var forbiddenManufacturerNames = blackList.Select(m => m.name).ToArray();
                list.RemoveAll(priceLine => forbiddenManufacturerNames.Contains(priceLine.Manufacturer, StringComparer.OrdinalIgnoreCase));
            }

            //Применяем белый лист
            var whiteList = templateInfo.manufacturer_skip_list.Where(i => i.list_type == "white_list").ToArray();
            if (whiteList.Length > 0)
            {
                var allowedManufacturerNames = whiteList.Select(m => m.name).ToArray();
                list.RemoveAll(priceLine => !allowedManufacturerNames.Contains(priceLine.Name, StringComparer.OrdinalIgnoreCase));

            }

        }

        /// <summary>
        /// Заполняем связанных склад поставщика для всех считанных строк
        /// </summary>
        /// <param name="list"></param>
        /// <param name="stock_partner_id"></param>
        private void FillLinkedStock(List<PriceLine> list, int? stock_partner_id)
        {
            if (!stock_partner_id.HasValue)
            {
                return;
            }

            StockName stock = (StockName)stock_partner_id.Value;
            foreach (var line in list.Where(l => l.Stock == StockName.None))
            {
                line.Stock = stock;
            }
        }

        /// <summary>
        /// Подменяем модели в считанных строках из прайс-листа, если есть добавленные сопоставления
        /// </summary>
        /// <param name="priceLines"></param>
        /// <param name="templateInfo"></param>
        private void ApplyModelMap(IEnumerable<PriceLine> priceLines, PriceListTemplateEntity templateInfo)
        {
            if (templateInfo.model_map == null || templateInfo.model_map.Count == 0)
            {
                return;
            }

            var modelMap = templateInfo.model_map.ToDictionary(i => i.old_text, i => i.new_text);

            logger.LogInformation($"Запуск преобразования моделей/артикулов для прайс-листа '{templateInfo?.title ?? "<Пусто>"}'");

            var sw = new Stopwatch();

            var linesToReplace = priceLines
                .Where(line =>
                    line.Model != null && modelMap.ContainsKey(line.Model) ||
                    line.Sku != null && modelMap.ContainsKey(line.Sku))
                .ToList();

            foreach (var line in linesToReplace)
            {
                if (modelMap.ContainsKey(line.Model))
                {
                    line.Model = modelMap[line.Model];
                }
                if (modelMap.ContainsKey(line.Sku))
                {
                    line.Sku = modelMap[line.Sku];
                }
            }

            logger.LogInformation($"Конец преобразования моделей/артикулов для прайс-листа '{templateInfo?.title ?? "<Пусто>"}'. Длительность выполнения: {sw.Elapsed.TotalMilliseconds:N} ms");
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
            source.ToList().ForEach(line => line.OriginalPrice = line.Price);


            bool emptyDiscountGeneralDiscount = templateInfo.discount == decimal.Zero;
            bool emptySellDiscountMap = templateInfo.manufacturer_discount_map.Count == 0;
            bool emptyPurchaseDiscountMap = templateInfo.manufacturer_purchase_map.Count == 0;

            // берем цену из прайс-листа как есть. Т.к. нет ни наценки, ни НДС
            if (emptyDiscountGeneralDiscount && emptySellDiscountMap && emptyPurchaseDiscountMap && !templateInfo.nds)
            {
                return;
            }

            // Наценка продажи Бренд/Скидка
            Dictionary<string, decimal> sellDiscounts = templateInfo.manufacturer_discount_map.ToDictionary(i => i.name, i => i.discount);

            // Наценка закупки Бренд/Скидка
            Dictionary<string, decimal> purchaseDiscounts = templateInfo.manufacturer_purchase_map.ToDictionary(i => i.name, i => i.discount);

            //Загружаем размер НДС из настроек
            NDS = (100m + await settings.GetValue<int>("nds")) / 100;

            foreach (var line in source)
            {
                CaclDiscount(templateInfo, emptyDiscountGeneralDiscount, emptySellDiscountMap, emptyPurchaseDiscountMap, sellDiscounts, purchaseDiscounts, line);
            }
        }

        /// <summary>
        /// Расчитать скидку для определенной строки из прайс-листа
        /// </summary>
        /// <param name="templateInfo"></param>
        /// <param name="emptyDiscountGeneralDiscount"></param>
        /// <param name="emptySellDiscountMap"></param>
        /// <param name="emptyPurchaseDiscountMap"></param>
        /// <param name="sellDiscounts"></param>
        /// <param name="purchaseDiscounts"></param>
        /// <param name="line"></param>
        private void CaclDiscount(PriceListTemplateEntity templateInfo,
                bool emptyDiscountGeneralDiscount,
                bool emptySellDiscountMap,
                bool emptyPurchaseDiscountMap,
                IReadOnlyDictionary<string, decimal> sellDiscounts,
                IReadOnlyDictionary<string, decimal> purchaseDiscounts,
                PriceLine line)
        {
            //Сначала добавляет к цене NDS, если нужно
            if (templateInfo.nds)
            {
                line.Price = AddNds(line.Price.Value, line.Currency);
                line.OriginalPrice = AddNds(line.OriginalPrice.Value, line.Currency);
            }

            decimal discountValue = emptyDiscountGeneralDiscount ? 0m : templateInfo.discount;
            // Если бренд есть в словаре, то берем скидку для него в первую очередь (заменяя стандартную от прайс-листа)
            if (!emptySellDiscountMap && sellDiscounts.ContainsKey(line.Manufacturer))
            {
                discountValue = sellDiscounts[line.Manufacturer];
            }

            if (!emptyPurchaseDiscountMap && discountValue != 0 && purchaseDiscounts.TryGetValue(line.Manufacturer, out var pdv))
            {
                line.Price *= 1m - pdv / 100m;
            }

            //Высчитываем множитель
            decimal discountRatio = 1m + discountValue / 100m;
            if (discountRatio != 1)
            {
                line.Price = Math.Round(line.Price.Value * discountRatio, line.Currency == CurrencyType.RUB ? 0 : 2, MidpointRounding.ToZero);
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

        /// <summary>
        /// Загружаем описание шаблонапо указанному типа
        /// </summary>
        /// <param name="templateType"></param>
        /// <returns></returns>
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

                    //Если товар уже загружен - то прибавляем остаток от новой загруженно строки прайс-листа (без добавления)
                    continue;
                }

                PriceLines.Add(line);
            }
        }

        /// <summary>
        /// Удаляем из памяти все ранее загруженные прайс-листы
        /// </summary>
        public void RemovePriceListAll()
        {
            LoadedFiles.Clear();
            PriceLines.Clear();
        }

        /// <summary>
        /// Удаляем из памяти данные загруженного прайс-листа определенного типа
        /// </summary>
        /// <param name="data"></param>
        public void RemovePriceList(LoadedPriceListTemplateData data)
        {
            LoadedFiles.Remove(data);
            PriceLines.RemoveAll(line => line.Template == data.TemplateInstance);
        }
    }
}