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
        private readonly ISettingStorage settingStorage;
        private readonly IManufacturerStorage manufacturerStorage;
        private readonly IDatabaseProductCorrelator correlator;

        public UpdateManager(IProductStorage productsStorage, 
            ISettingStorage settingStorage,
            IManufacturerStorage manufacturerStorage, 
            IDatabaseProductCorrelator correlator)
        {
            this.productsStorage = productsStorage;
            this.settingStorage = settingStorage;
            this.manufacturerStorage = manufacturerStorage;
            this.correlator = correlator;
        }

        public async Task UpdatePriceAndStock(
            IEnumerable<PriceLine> priceLines, 
            bool clearStockBeforeUpdate, 
            IProgress<string> progress = null)
        {
            //TODO тут надо будет проверить, т.к. нужно следить что бы в прайс-листах (и шаблонах соответственно) имя производителя точно совпадало с именем из базы данных 
            //Иначе товары не будут загружены и соответственно не обновлены
            //Проблема с Bosch, и возможно другими брендами где pl.manufacturer = null

            progress?.Report("Подготовка списка производителей для обновления");
            await Task.Delay(TimeSpan.FromSeconds(1));
            var affectedBrands = priceLines.Select(pl => pl.Manufacturer).Distinct().ToList();
            var affectedBrandsIds = (await manufacturerStorage.GetManufacturers())
                .Where(m => affectedBrands.Contains(m.name))
                .Select(m => m.manufacturer_id)
                .ToList();

            progress?.Report("Загрузка товаров");
            await Task.Delay(TimeSpan.FromSeconds(1));
            var products = await productsStorage.ReadProducts(affectedBrandsIds);
       
            progress?.Report("Сопоставление товаров для обновления");
            await Task.Delay(TimeSpan.FromSeconds(1));
            var data = await correlator.GetCorrelationData(products, priceLines);
            
            progress?.Report("Обновление цен etk-komplekt.ru");
            await Task.Delay(TimeSpan.FromSeconds(1));
            await productsStorage.UpdateProductsPrice(data);

            progress?.Report("Обновление остатков etk-komplekt.ru");
            await Task.Delay(TimeSpan.FromSeconds(1));
            await productsStorage.UpdateProductsStock(data, clearStockBeforeUpdate);

            if (data.Any(line => line.price.HasValue))
            {
                progress?.Report("Пересчет цен товаров в валюте");
                await Task.Delay(TimeSpan.FromSeconds(1));
                await new WebClient().DownloadStringTaskAsync(CurrencyPlusUri);
            }

            progress?.Report("Обновление монобренд сайтов");
            await Task.Delay(TimeSpan.FromSeconds(1));
            await UpdateMonobrands(affectedBrandsIds, progress);

            progress.Report("Обновление завершено");
        }

        private async Task UpdateMonobrands(List<int> affectedBrandsIds, IProgress<string> progress = null)
        {
            var monobrands = await manufacturerStorage.GetMonobrands();
            var affectedMonobrands = monobrands
                .Where(monobrand => affectedBrandsIds.Contains(monobrand.manufacturer_id))
                .ToList();

            if(affectedMonobrands.Count == 0) { return; }        

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
