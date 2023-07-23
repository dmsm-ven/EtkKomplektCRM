using EtkBlazorApp.BL;
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

    private readonly string currencyXmlFeed = "http://www.cbr.ru/scripts/XML_daily.asp";
    private readonly string cacheKey = "currency_rates";

    public CurrencyCheckerCbRf_V2(IMemoryCache cache, SystemEventsLogger logger)
    {
        this.cache = cache;
        this.logger = logger;
    }

    public async ValueTask<decimal> GetCurrencyRate(CurrencyType type)
    {
        if (!cache.TryGetValue<Dictionary<CurrencyType, decimal>>(cacheKey, out var rates))
        {
            try
            {
                rates = await ReadCurrenciesFromCbRf();
                LastUpdate = DateTime.Now;
                cache.Set(cacheKey, rates, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            }
            catch (Exception ex)
            {
                await logger.WriteSystemEvent(LogEntryGroupName.Auth, "Ошибка курса валют", $"Не удалось загрузить курс валют из источника: {currencyXmlFeed} Детали: {ex.Message}");
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

        var doc = await Task.Run(() => XDocument.Load(currencyXmlFeed));

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

