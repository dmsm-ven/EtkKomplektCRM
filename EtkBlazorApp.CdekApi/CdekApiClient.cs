using EtkBlazorApp.Core.Data.Cdek;
using EtkBlazorApp.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EtkBlazorApp.CdekApi;

public class CdekApiMemoryCachedClient : ITransportCompanyApi
{
    private readonly IMemoryCache memoryCache;
    private readonly MemoryCacheEntryOptions cacheEntryOptions;
    private readonly TimeSpan CacheExpireTime = TimeSpan.FromSeconds(90);
    private readonly string account;
    private readonly string securePassword;
    private readonly HttpClient httpClient;

    private DateTime? tokenExpireTime;

    public CdekApiMemoryCachedClient(IConfiguration configuration, IMemoryCache memoryCache)
    {
        var section = configuration.GetSection("CdekConfiguration");
        this.account = section["Account"] ?? throw new ArgumentNullException("Account");
        this.securePassword = section["SecurePassword"] ?? throw new ArgumentNullException("SecurePassword");

        this.memoryCache = memoryCache;
        cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheExpireTime);

        httpClient = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            CookieContainer = new System.Net.CookieContainer(),
        });
        //httpClient.BaseAddress = new Uri("https://api.edu.cdek.ru"); // тестовая версия
        httpClient.BaseAddress = new Uri("https://api.cdek.ru");
        httpClient.DefaultRequestHeaders
          .Accept
          .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task AuthorizeIfNeed()
    {
        if (tokenExpireTime.HasValue && tokenExpireTime > DateTime.Now)
        {
            return;
        }

        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", account),
            new KeyValuePair<string, string>("client_secret", securePassword),
        };

        var response = await httpClient.PostAsync("/v2/oauth/token?parameters", new FormUrlEncodedContent(data));
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Ошибка обращения к СДЭК API");
        }

        var authResult = await response.Content.ReadAsAsync<CdekAuthResult>();

        tokenExpireTime = DateTime.Now.AddSeconds(authResult.expires_in);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.access_token);
    }

    public async Task<CdekOrderInfo> GetOrderInfo(string cdekOrderNumber)
    {
        string key = $"cdek_api_order_info_{cdekOrderNumber}";

        if (!memoryCache.TryGetValue<CdekOrderInfo>(key, out var info))
        {
            await AuthorizeIfNeed();

            var response = await httpClient.GetAsync($"/v2/orders?cdek_number={cdekOrderNumber}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var orderInfo = JsonSerializer.Deserialize<CdekOrderInfoRoot>(json);
                var lastVersion = orderInfo.entity;


                memoryCache.Set<CdekOrderInfo>(key, lastVersion, cacheEntryOptions);

                return lastVersion;
            }
        }
        return info;
    }
}

