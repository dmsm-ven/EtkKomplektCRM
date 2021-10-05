using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.Integration.Ozon
{
    //Можно удалять, скорее всего использовать больше не будет
    [Obsolete("Перенесено в API controller на сайте для загрузки через фид. /controller/feed/ozon_seller")]
    public class OzonSellerApiClient
    {
        const int MAX_PRODUCTS_PER_PAGE = 1000;
        const int MAX_PRICE_ITEMS_PER_REQUEST = 1000;
        const int MAX_STOCK_ITEMS_PER_REQUEST = 100;

        readonly HttpClient client;
       
        public OzonSellerApiClient(string client_id, string api_key)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://api-seller.ozon.ru/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Client-Id", client_id);
            client.DefaultRequestHeaders.Add("Api-Key", api_key);
            client.DefaultRequestHeaders.Host = "api-seller.ozon.ru";

        }

        /// <summary>
        /// https://cb-api.ozonru.me/apiref/ru/#t-title_get_products_list
        /// </summary>
        /// <returns></returns>
        public async Task<List<OzonProductModel>> GetAllProducts()
        {
            var list = new List<OzonProductModel>();
            int currentPage = 1;
            int totalPages = 1;

            do
            {
                if (currentPage > 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                var filter = new { filter = new { }, page_size = MAX_PRODUCTS_PER_PAGE.ToString(), page = currentPage };
                var jsonString = JsonConvert.SerializeObject(filter);
                var content = new StringContent(jsonString, UnicodeEncoding.UTF8, "application/json");

                var response = await client.PostAsync("v1/product/list", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var root = JsonConvert.DeserializeObject<OzonProductListRootObjectModel>(json);
                    list.AddRange(root.result.items);

                    if (currentPage == 1)
                    {
                        totalPages = (int)Math.Ceiling((double)root.result.total / MAX_PRODUCTS_PER_PAGE);
                    }
                }

                currentPage++;

            } while (currentPage <= totalPages);

            return list;
        }

        /// <summary>
        /// https://cb-api.ozonru.me/apiref/ru/#t-title_post_products_prices
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task UpdatePrice(Dictionary<OzonProductModel, decimal> data, IProgress<Tuple<int, int>> progress = null)
        {
            if (!data.Any()) { return; }

            int currentPage = 1;
            int totalPages = (int)Math.Ceiling((double)data.Count / MAX_PRICE_ITEMS_PER_REQUEST);

            do
            {
                if (currentPage > 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                var sendData = new { prices = new List<object>() };

                var source = data.Skip((currentPage - 1) * MAX_PRICE_ITEMS_PER_REQUEST).Take(MAX_PRICE_ITEMS_PER_REQUEST);
                foreach (var d in source)
                {
                    sendData.prices.Add(new { offer_id = d.Key.offer_id, price = d.Value.ToString(), old_price = d.Value.ToString() });
                }


                var jsonString = JsonConvert.SerializeObject(sendData);
                var content = new StringContent(jsonString, UnicodeEncoding.UTF8, "application/json");

                var response = await client.PostAsync("v1/product/import/prices", content);
                if (response.IsSuccessStatusCode)
                {
                    var serverJsonResponse = await response.Content.ReadAsStringAsync();
                }

                progress?.Report(Tuple.Create(currentPage, totalPages));
                currentPage++;
            } while (currentPage <= totalPages);
            progress?.Report(Tuple.Create(totalPages, totalPages));
        }

        /// <summary>
        /// https://cb-api.ozonru.me/apiref/ru/#t-title_post_products_stocks
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task UpdateQuantity(Dictionary<OzonProductModel, int> data, IProgress<Tuple<int, int>> progress = null)
        {
            if (!data.Any()) { return; }

            int currentPage = 1;
            int totalPages = (int)Math.Ceiling((double)data.Count / MAX_STOCK_ITEMS_PER_REQUEST);

            do
            {
                if (currentPage > 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                var sendData = new { stocks = new List<object>() };
                var source = data.Skip((currentPage - 1) * MAX_STOCK_ITEMS_PER_REQUEST).Take(MAX_STOCK_ITEMS_PER_REQUEST);
                foreach (var d in source)
                {
                    sendData.stocks.Add(new { offer_id = d.Key.offer_id, stock = Math.Max(d.Value, 0) });
                }


                var jsonString = JsonConvert.SerializeObject(sendData);
                var content = new StringContent(jsonString, UnicodeEncoding.UTF8, "application/json");

                var response = await client.PostAsync("v1/product/import/stocks", content);
                if (response.IsSuccessStatusCode)
                {
                    var serverJsonResponse = await response.Content.ReadAsStringAsync();
                }
                progress?.Report(Tuple.Create(currentPage, totalPages));
                currentPage++;
            } while (currentPage <= totalPages);
            progress?.Report(Tuple.Create(totalPages, totalPages));
        }
    }
}
