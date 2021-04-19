using System;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class DefaultRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly string remoteUri;

        internal DefaultRemoteTemplateFileLoader(string remoteUri)
        {
            this.remoteUri = remoteUri;
        }

        public async Task<byte[]> GetBytes()
        {
            using (var wc = new WebClient())
            {
                var bytes = await Task.Run(() => wc.DownloadData(new Uri(remoteUri)));
                return bytes;
            }
        }
    }
}
