using EtkBlazorApp.DataAccess.Repositories.PriceList;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.BL
{
    public class MksMasterProApiFileLoader : IRemoteTemplateFileLoader
    {
        static readonly int ETK_KOMPLEKT_LK_ID = 598510;
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly string guid;

        internal MksMasterProApiFileLoader(IPriceListTemplateStorage templateStorage, string guid)
        {
            this.templateStorage = templateStorage;
            this.guid = guid;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            var templateInfo = await templateStorage.GetPriceListTemplateById(guid);
            string login = templateInfo.credentials_login;
            string password = templateInfo.credentials_password;

            var handler = new HttpClientHandler()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,

                //AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler) { BaseAddress = new Uri("https://mks.master.pro") })
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");

                //Логинимся
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", login),
                    new KeyValuePair<string, string>("password", password)
                });
                var loginReponse = await client.PostAsync("/login", formContent);

                //Скачиваем прайс-лист
                var obj = new { contragent = ETK_KOMPLEKT_LK_ID };
                var jsonContent = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                using (var response = await client.PostAsync("/api/getPriceList", jsonContent))
                {
                    var contentBytes = await response.Content.ReadAsByteArrayAsync();
                    var str = Encoding.UTF8.GetString(contentBytes);

                    //Очищаем от ненужных заголовков и экранируем
                    str = str
                        .Substring(str.LastIndexOf(',') + 1)
                        .Trim('"')
                        .Replace(@"\/", "/");

                    byte[] bytes = Convert.FromBase64String(str);

                    return new RemoteTemplateFileResponse(bytes, "eridan_etk_komplekt.xlsx");
                }
            }
        }
    }
}
