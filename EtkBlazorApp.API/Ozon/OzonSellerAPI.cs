using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace EtkBlazorApp.API.Ozon
{
    //Актуальная (рабочая) версия в EtkKomplekt windows приложении
    public class OzonSellerApi
    {
        private readonly IConfiguration configuration;

        private string SELLER_ID => configuration.GetSection("ozon")["client_id"];
        private string API_KEY => configuration.GetSection("ozon")["api_key"];

        const string MAX_PRODUCTS_PER_PAGE = "1000";

        readonly HttpClient client;

        public OzonSellerApi(IConfiguration configuration)
        {
            this.configuration = configuration;
            client = new HttpClient();
            client.BaseAddress = new Uri("https://api-seller.ozon.ru/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("Client-Id", SELLER_ID);
            client.DefaultRequestHeaders.Add("Api-Key", API_KEY);
            client.DefaultRequestHeaders.Host = "api-seller.ozon.ru";

        }

        //https://cb-api.ozonru.me/apiref/ru/#t-title_get_products_list
        public async Task<List<OzonProductModel>> GetAllProducts()
        {
            var list = new List<OzonProductModel>();
            int currentPage = 1;
            int totalPages = 1;

            do
            {
                var filter = new { filter = new { }, page_size = MAX_PRODUCTS_PER_PAGE, page = currentPage };
                var jsonString = JsonConvert.SerializeObject(filter);
                var content = new StringContent(jsonString, UnicodeEncoding.UTF8, "application/json");

                var response = await client.PostAsync("v1/product/list", content); 
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<OzonProductListRootObjectModel>();
                    list.AddRange(model.result.items);

                    if (currentPage == 1)
                    {
                        totalPages = (int)Math.Ceiling((double)model.result.total / 1000);
                    }
                }

                currentPage++;

            } while (currentPage <= totalPages);

            return list;
        }

        //https://cb-api.ozonru.me/apiref/ru/#t-title_post_products_prices
        public async Task UpdatePrice(Dictionary<OzonProductModel, decimal> data)
        {

        }

        //https://cb-api.ozonru.me/apiref/ru/#t-title_post_products_stocks
        public async Task UpdateQuantity(Dictionary<OzonProductModel, int> data)
        {

        }
    }
}
