using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class HttpGetRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly string remoteUri;

        internal HttpGetRemoteTemplateFileLoader(string remoteUri)
        {
            this.remoteUri = remoteUri;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            using (var wc = new WebClient())
            {
                var bytes = await Task.Run(() => wc.DownloadData(new Uri(remoteUri)));
                string fileName = Path.GetFileName(remoteUri);

                return new RemoteTemplateFileResponse(bytes, fileName);
            }
        }
    }
}
