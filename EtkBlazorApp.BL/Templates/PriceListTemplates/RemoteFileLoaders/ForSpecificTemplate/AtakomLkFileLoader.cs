using EtkBlazorApp.DataAccess.Repositories.PriceList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders.ForSpecificTemplate
{
    public class AtakomLkFileLoader : IRemoteTemplateFileLoader
    {
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly string guid;

        internal AtakomLkFileLoader(IPriceListTemplateStorage templateStorage, string guid)
        {
            this.templateStorage = templateStorage;
            this.guid = guid;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            var templateInfo = await templateStorage.GetPriceListTemplateById(guid);
            string login = templateInfo.credentials_login;
            string password = templateInfo.credentials_password;
            string priceListUri = templateInfo.remote_uri;

            var handler = new HttpClientHandler()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://www.aktakom.ru")
            };

            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");

            var loginValue = HttpUtility.UrlDecode("%C2%EE%E9%F2%E8");

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("backurl", "/index.php"),
                new KeyValuePair<string, string>("AUTH_FORM", "Y"),
                new KeyValuePair<string, string>("TYPE", "AUTH"),
                new KeyValuePair<string, string>("USER_LOGIN", login),
                new KeyValuePair<string, string>("USER_PASSWORD", password),
                new KeyValuePair<string, string>("Login", loginValue),
            });

            var loginReponse = await client.PostAsync("https://www.aktakom.ru/index.php?login=yes", formContent);

            byte[] bytes = await client.GetByteArrayAsync(priceListUri);

            client.Dispose();

            var fileNameRegex = Regex.Match(Path.GetFileName(priceListUri), "Y&file=(.*?)$"); //пример: price_aktakom_dealer_7811658788.csv
            string fileName = fileNameRegex.Success && fileNameRegex.Groups.Count > 1 ? fileNameRegex.Groups[1].Value : "aktakom.csv";
            return new RemoteTemplateFileResponse(bytes, fileName);
        }
    }
}
