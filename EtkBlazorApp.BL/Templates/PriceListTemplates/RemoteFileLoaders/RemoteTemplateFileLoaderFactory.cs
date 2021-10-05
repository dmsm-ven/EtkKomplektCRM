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
        private readonly ISettingStorage settings;
        private readonly ICompressedFileExtractor zipExtractor;
        private readonly IPriceListTemplateStorage templateStorage;

        public RemoteTemplateFileLoaderFactory(ISettingStorage storage, 
            ICompressedFileExtractor zipExtractor, 
            IPriceListTemplateStorage templateStorage)
        {
            this.settings = storage;
            this.zipExtractor = zipExtractor;
            this.templateStorage = templateStorage;
        }

        public IRemoteTemplateFileLoader GetMethod(string remoteUri, string methodName, string guid)
        {
            switch (methodName)
            {
                case "HttpGet":
                    return new HttpGetRemoteTemplateFileLoader(remoteUri);
                case "HttpGetWithCredentials":
                    return new HttpGetWithCredentialsRemoteTemplateFileLoader(templateStorage, guid);
                case "YandexDisk":
                    return new YandexDiskRemoteTemplateFileLoader(remoteUri, zipExtractor);
                case "EmailAttachment":
                    return new EmailAttachmentRemoteTemplateFileLoader(settings, zipExtractor, templateStorage, guid);
                case "mks.master.pro API":
                    return new MksMasterProApiFileLoader(templateStorage, guid);
            }

            throw new NotSupportedException($"Шаблон загрузки файла '{methodName}' не поддерживается");
        }
    }
}
