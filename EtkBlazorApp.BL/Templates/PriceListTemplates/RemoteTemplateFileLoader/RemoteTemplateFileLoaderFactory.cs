using EtkBlazorApp.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates
{ 
    public class RemoteTemplateFileLoaderFactory
    {
        private readonly ISettingStorage storage;

        public RemoteTemplateFileLoaderFactory(ISettingStorage storage)
        {
            this.storage = storage;
        }

        public IRemoteTemplateFileLoader GetMethod(string remoteUri, string methodName = null, string guid = null)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return new DefaultRemoteTemplateFileLoader(remoteUri);
            }

            switch (methodName)
            {
                case "HttpGet":
                    return new DefaultRemoteTemplateFileLoader(remoteUri);
                case "HttpGetWithCredentials":
                    return new HttpGetWithCredentialsRemoteTemplateFileLoader(remoteUri, guid, storage);
                case "YandexDisk":
                    return new YandexDiskRemoteTemplateFileLoader(remoteUri);
            }

            throw new NotSupportedException($"Шаблон '{methodName}' не поддерживается");
        }
    }
}
