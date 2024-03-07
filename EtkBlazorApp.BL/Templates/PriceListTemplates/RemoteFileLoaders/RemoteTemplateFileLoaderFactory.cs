using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using System;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders
{
    public class RemoteTemplateFileLoaderFactory
    {
        private readonly ISettingStorageReader settings;
        private readonly ICompressedFileExtractor zipExtractor;
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly EncryptHelper encryptHelper;

        public RemoteTemplateFileLoaderFactory(ISettingStorageReader settings,
            ICompressedFileExtractor zipExtractor,
            IPriceListTemplateStorage templateStorage,
            EncryptHelper encryptHelper)
        {
            this.settings = settings;
            this.zipExtractor = zipExtractor;
            this.templateStorage = templateStorage;
            this.encryptHelper = encryptHelper;
        }

        public IRemoteTemplateFileLoader GetMethod(string remoteUri, string methodName, string guid)
        {
            //методы перечислены в таблице 'etk_app_price_list_template_remote_method'
            switch (methodName)
            {
                case "HttpGet":
                    return new HttpClientGetRemoteTemplateFileLoader(remoteUri);
                case "HttpGetWithCredentials":
                    return new HttpGetWithCredentialsRemoteTemplateFileLoader(templateStorage, guid);
                case "HttpGetWithProxy":
                    return new HttpGetWithProxyRemoteTemplateFileLoader(templateStorage, guid);
                case "YandexDisk":
                    return new YandexDiskRemoteTemplateFileLoader(remoteUri, zipExtractor);
                case "EmailAttachment":
                    return new EmailAttachmentRemoteTemplateFileLoader(
                        templateStorage,
                        new EmailAttachmentExtractorInitializer(settings, zipExtractor, encryptHelper),
                        guid);
                case "mks.master.pro API":
                    return new MksMasterProApiFileLoader(templateStorage, guid);
            }

            throw new NotSupportedException($"Шаблон загрузки файла '{methodName}' не поддерживается");
        }
    }
}
