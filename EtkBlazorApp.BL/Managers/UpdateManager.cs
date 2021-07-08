using EtkBlazorApp.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class UpdateManager
    {
        //Запуск пересчета цен товаров в валюте
        readonly string CurrencyPlusUri = "https://etk-komplekt.ru/cron/currency_plus.php";

        private readonly IProductStorage productsStorage;
        private readonly IProductUpdateService productUpdateService;
        private readonly ISettingStorage settingStorage;
        private readonly IManufacturerStorage manufacturerStorage;
        private readonly IDatabaseProductCorrelator correlator;

        public UpdateManager(IProductStorage productsStorage,
            IProductUpdateService productUpdateService,
            ISettingStorage settingStorage,
            IManufacturerStorage manufacturerStorage, 
            IDatabaseProductCorrelator correlator)
        {
            this.productsStorage = productsStorage;
            this.productUpdateService = productUpdateService;
            this.settingStorage = settingStorage;
            this.manufacturerStorage = manufacturerStorage;
            this.correlator = correlator;
        }

        public async Task UpdatePriceAndStock(
            IEnumerable<PriceLine> priceLines, 
            IProgress<string> progress = null)
        {
            progress?.Report("Подготовка списка производителей для обновления");
            int [] affectedBrandsIds = await GetAffectedBrandIds(priceLines);

            progress?.Report("Загрузка товаров");
            var products = await productsStorage.ReadProducts(affectedBrandsIds);

            progress?.Report("Сопоставление товаров для обновления");
            var data = await correlator.GetCorrelationData(products, priceLines);

            progress?.Report("Обновление цен etk-komplekt.ru");
            await productUpdateService.UpdateProductsPrice(data);

            data.Where(d => d.stock_partner == null).ToList().ForEach(i => i.stock_partner = (int?)StockPartner.Symmetron);
            progress?.Report("Обновление остатков на складах etk-komplekt.ru");
            await productUpdateService.UpdateProductsStockPartner(data, affectedBrandsIds);

            progress?.Report("Складывание остатков складов etk-komplekt.ru");
            await productUpdateService.ComputeStockQuantity(data);

            if (data.Any(line => line.price.HasValue) && data.Any(pl => pl.currency_code != "RUB"))
            {
                progress?.Report("Пересчет цен товаров в валюте");
                await new WebClient().DownloadStringTaskAsync(CurrencyPlusUri);
            }

            await UpdateMonobrands(affectedBrandsIds, progress);
            
            progress?.Report("Обновление завершено");
        }

        private async Task<int[]> GetAffectedBrandIds(IEnumerable<PriceLine> priceLines)
        {
            var allManufacturers = await manufacturerStorage.GetManufacturers();

            var affectedBrands = priceLines
                .Select(pl => pl.Manufacturer)
                .Distinct()
                .ToList();

            AddAdditionalAffectedBrands(affectedBrands);

            var affectedBrandsIds = allManufacturers
                .Where(m => affectedBrands.Contains(m.name, StringComparer.OrdinalIgnoreCase))
                .Select(m => m.manufacturer_id)
                .ToList();

            return affectedBrandsIds.ToArray();
        }

        private void AddAdditionalAffectedBrands(List<string> affectedBrands)
        {
            // В производителе Bosch есть подбренд Dremel, но он так же считается и отдельным брендом. 
            // Хотя находится в прайс-листе Bosch и там не указан как отдельный бренд
            if (affectedBrands.Contains("Bosch"))
            {
                affectedBrands.Add("Dremel");
            }

            // Если есть какой-либо бренд из списка то добавляем всех, т.к. у них есть товары которые по факту один и тот же
            // Но называется в каждом из брендов по разному и у всех одна модель (на сайте есть только 1 вариант)
            var eltechBrands = new string[] { "Tianma", "BOE", "NEC", "AU Optronics" };
            if (eltechBrands.Any(eltechBrand => affectedBrands.Contains(eltechBrand)))
            {
                affectedBrands.AddRange(eltechBrands);
            }
        }

        private async Task UpdateMonobrands(IEnumerable<int> affectedBrandsIds, IProgress<string> progress = null)
        {
            var monobrands = await manufacturerStorage.GetMonobrands();
            var affectedMonobrands = monobrands
                .Where(monobrand => affectedBrandsIds.Contains(monobrand.manufacturer_id))
                .ToList();

            if(affectedMonobrands.Count == 0) { return; }

            progress?.Report("Обновление монобренд сайтов");
            string key = await settingStorage.GetValue("monobrand_updater_key");
            foreach (var monobrand in affectedMonobrands)
            {
                try
                {
                    string uriQuery = $"client_name=monobrand&key={key}&manufacturer={monobrand.manufacturer_name}&currency_code={monobrand.currency_code}";
                    string apiUri = $"{monobrand.website}/?route=api/monobrand_updater/update&{uriQuery}";
              
                    progress?.Report($"Обновление сайта {monobrand.website}");
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    var result = await new WebClient().DownloadStringTaskAsync(apiUri);
                }
                catch(Exception ex)
                {
                    progress?.Report($"Ошибка обновления сайта {monobrand.website}. " + ex.Message);
                }
            }
        }
    }
}
