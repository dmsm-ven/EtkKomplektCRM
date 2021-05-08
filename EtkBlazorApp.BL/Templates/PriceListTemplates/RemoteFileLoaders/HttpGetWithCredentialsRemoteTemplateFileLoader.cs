using EtkBlazorApp.DataAccess;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class HttpGetWithCredentialsRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly string guid;

        internal HttpGetWithCredentialsRemoteTemplateFileLoader(IPriceListTemplateStorage templateStorage, string guid)
        {
            this.templateStorage = templateStorage;
            this.guid = guid;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            var templateInfo = await templateStorage.GetPriceListTemplateById(guid);
            string login = templateInfo.credentials_login;
            string password = templateInfo.credentials_password;
            string remoteUri = templateInfo.remote_uri;

            using (var wc = new WebClient())
            {
                wc.Credentials = new NetworkCredential(login, password);

                var bytes = await Task.Run(() => wc.DownloadData(new Uri(remoteUri)));
                string fileName = Path.GetFileName(remoteUri);

                return new RemoteTemplateFileResponse(bytes, fileName);
            }
        }
    }
}
