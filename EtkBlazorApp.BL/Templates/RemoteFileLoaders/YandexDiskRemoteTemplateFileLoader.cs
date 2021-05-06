using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.BL
{
    public class YandexDiskRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly string remoteUri;

        internal YandexDiskRemoteTemplateFileLoader(string remoteUri)
        {
            this.remoteUri = remoteUri;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            using (var wc = new WebClient())
            {
                string requestUri = "https://cloud-api.yandex.net:443/v1/disk/public/resources/download?public_key=" + HttpUtility.UrlEncode(remoteUri);

                var jsonStringResponse = await wc.DownloadStringTaskAsync(requestUri);
                var jsonObject = JsonConvert.DeserializeObject<YandexDiskApiResponse>(jsonStringResponse);

                if (!string.IsNullOrEmpty(jsonObject.href))
                {
                    var bytes = await Task.Run(() => wc.DownloadData(jsonObject.href));

                    var uriQuery = new Uri(HttpUtility.UrlDecode(jsonObject.href)).Query;
                    var fileName = HttpUtility.ParseQueryString(uriQuery)["filename"];

                    return new RemoteTemplateFileResponse(bytes, fileName);
                }
            }
            throw new WebException("Не удалось скачать файл по ссылке: " + remoteUri);
        }

        private class YandexDiskApiResponse
        {
            public string href { get; set; }
            public string method { get; set; }
            public bool templated { get; set; }
        }
    }

}
