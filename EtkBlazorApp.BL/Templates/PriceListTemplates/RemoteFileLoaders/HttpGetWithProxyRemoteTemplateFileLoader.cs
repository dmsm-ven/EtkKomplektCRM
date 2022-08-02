using EtkBlazorApp.DataAccess;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class HttpGetWithProxyRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly string guid;

        internal HttpGetWithProxyRemoteTemplateFileLoader(IPriceListTemplateStorage templateStorage, string guid)
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
                var proxy = new WebProxy(login);
                proxy.Credentials = new NetworkCredential(password.Split('@')[0], password.Split('@')[1]);
                wc.Proxy = proxy;

                var bytes = await wc.DownloadDataTaskAsync(remoteUri);
                string fileName = Path.GetFileName(remoteUri);

                return new RemoteTemplateFileResponse(bytes, fileName);
            }
        }
    }
}
