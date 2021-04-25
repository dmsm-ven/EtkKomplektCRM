using EtkBlazorApp.DataAccess;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class HttpGetWithCredentialsRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly string remoteUri;
        private readonly string guid;
        private readonly ISettingStorage settingStorage;

        internal HttpGetWithCredentialsRemoteTemplateFileLoader(string remoteUri, string guid, ISettingStorage settingStorage)
        {
            this.remoteUri = remoteUri;
            this.guid = guid;
            this.settingStorage = settingStorage;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            using (var wc = new WebClient())
            {
                string login = await settingStorage.GetValue($"price-list-template-credentials-{guid}-login");
                string password = await settingStorage.GetValue($"price-list-template-credentials-{guid}-password");

                wc.Credentials = new NetworkCredential(login, password);

                var bytes = await Task.Run(() => wc.DownloadData(new Uri(remoteUri)));
                string fileName = Path.GetFileName(remoteUri);

                return new RemoteTemplateFileResponse(bytes, fileName);
            }
        }
    }

    


}
