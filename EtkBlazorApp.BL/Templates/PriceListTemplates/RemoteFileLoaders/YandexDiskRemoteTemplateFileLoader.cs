using EtkBlazorApp.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace EtkBlazorApp.BL
{
    public class YandexDiskRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly string remoteUri;
        private readonly ICompressedFileExtractor zipExtractor;

        internal YandexDiskRemoteTemplateFileLoader(string remoteUri, ICompressedFileExtractor zipExtractor)
        {
            this.remoteUri = remoteUri;
            this.zipExtractor = zipExtractor;
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

                    if (fileName.EndsWith(".zip") || fileName.EndsWith(".rar"))
                    {
                        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(fileName));
                        File.WriteAllBytes(tempFile, bytes);

                        var extractedFiles = await zipExtractor.UnzipAll(tempFile, deleteArchive: true);

                        fileName = extractedFiles.FirstOrDefault();
                        bytes = File.ReadAllBytes(fileName);
                    }

                    return new RemoteTemplateFileResponse(bytes, Path.GetFileName(fileName));
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
