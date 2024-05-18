using EtkBlazorApp.BL.Data;
using EtkBlazorApp.BL.Loggers;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtkBlazorApp.Services.CurrencyChecker;

public class CurrencyCheckerCbRf_V2 : ICurrencyChecker
{
    public DateTime LastUpdate { get; private set; }
    private readonly IMemoryCache cache;
    private readonly SystemEventsLogger logger;

    private readonly string XmlFeedUri = "http://www.cbr.ru/scripts/XML_daily.asp";

    //Временный костыль, т.к. похоже забанили на CBR по IP сервер
    private readonly string XmlFeedUriProxy = "http://90.156.211.247:14223/cbr_rates_proxy";
    private readonly string cacheKey = "currency_rates";

    private DateTime? LoadingErrorsLastDate { get; set; }
    //Период времени в течении которого перестаем обращатся за курсом валют
    private TimeSpan LoadingErrorStropInterval { get; } = TimeSpan.FromHours(1);
    private TimeSpan CacheExpirationTime { get; } = TimeSpan.FromMinutes(45);

    public CurrencyCheckerCbRf_V2(IMemoryCache cache, SystemEventsLogger logger)
    {
        this.cache = cache;
        this.logger = logger;
    }

    public async ValueTask<decimal> GetCurrencyRate(CurrencyType type)
    {
        if (LoadingErrorsLastDate.HasValue && LoadingErrorsLastDate.Value.Add(LoadingErrorStropInterval) > DateTime.Now)
        {
            return 0;
        }

        if (!cache.TryGetValue<Dictionary<CurrencyType, decimal>>(cacheKey, out var rates))
        {
            try
            {
                rates = await ReadCurrenciesFromCbRf();
                LastUpdate = DateTime.Now;
                LoadingErrorsLastDate = null;
                cache.Set(cacheKey, rates, new MemoryCacheEntryOptions().SetAbsoluteExpiration(CacheExpirationTime));

            }
            catch (Exception ex)
            {
                await logger.WriteSystemEvent(LogEntryGroupName.Auth, "Ошибка курса валют", $"Не удалось загрузить курс валют из источника: {XmlFeedUriProxy} Детали: {ex.Message}");
                LoadingErrorsLastDate = DateTime.Now;
            }
        }

        return (rates != null && rates.ContainsKey(type)) ? rates[type] : 0;
    }

    private async Task<Dictionary<CurrencyType, decimal>> ReadCurrenciesFromCbRf()
    {
        var dic = new Dictionary<CurrencyType, decimal>()
        {
            [CurrencyType.RUB] = 1
        };

        var doc = await Task.Run(() => XDocument.Load(XmlFeedUriProxy));

        var currencies = doc.Descendants("Valute");

        foreach (var currencyNode in currencies)
        {
            string id = currencyNode.LastAttribute.Value;
            switch (id)
            {
                case "R01239": dic[CurrencyType.EUR] = decimal.Parse(currencyNode.Element("Value").Value); break;
                case "R01235": dic[CurrencyType.USD] = decimal.Parse(currencyNode.Element("Value").Value); break;
            }

        }

        return dic;
    }


}

