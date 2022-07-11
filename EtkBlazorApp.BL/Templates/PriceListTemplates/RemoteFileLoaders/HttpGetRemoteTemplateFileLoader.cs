using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    //первая версия через старый WebClient
    public class HttpWebClientGetRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly string remoteUri;

        internal HttpWebClientGetRemoteTemplateFileLoader(string remoteUri)
        {
            this.remoteUri = remoteUri;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            using (var wc = new WebClient())
            {
                var bytes = await wc.DownloadDataTaskAsync(remoteUri);
                string fileName = Path.GetFileName(remoteUri);
                return new RemoteTemplateFileResponse(bytes, fileName);
            }
        }
    }


    //Версия через HttpClient
    public class HttpClientGetRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly string remoteUri;
        private readonly HttpClient httpClient;
        private readonly TimeSpan Timeout = TimeSpan.FromMinutes(15);

        internal HttpClientGetRemoteTemplateFileLoader(string remoteUri)
        {
            this.remoteUri = remoteUri;
            httpClient = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                //AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = new CookieContainer()
            });
            httpClient.Timeout = Timeout;
            //httpClient.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            //httpClient.DefaultRequestHeaders.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            //httpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
            //Версия user-agent от 06.07.2022
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            try
            {
                string fileName = Path.GetFileName(remoteUri);
                var response = await httpClient.GetAsync(remoteUri);
                var bytes = await response.Content.ReadAsByteArrayAsync();

                return new RemoteTemplateFileResponse(bytes, fileName);
            }
            finally
            {
                httpClient.Dispose();
            }           
        }
    }

}
