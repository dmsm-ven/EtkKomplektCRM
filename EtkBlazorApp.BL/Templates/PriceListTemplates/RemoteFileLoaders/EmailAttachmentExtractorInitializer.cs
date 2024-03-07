using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class EmailAttachmentExtractorInitializer
    {
        private readonly ISettingStorageReader settingStorage;
        private readonly ICompressedFileExtractor zipExtractor;
        private readonly EncryptHelper encryptHelper;

        public EmailAttachmentExtractorInitializer(ISettingStorageReader settingStorage, ICompressedFileExtractor zipExtractor, EncryptHelper encryptHelper)
        {
            this.settingStorage = settingStorage;
            this.zipExtractor = zipExtractor;
            this.encryptHelper = encryptHelper;
        }

        public async Task<EmailAttachmentExtractor> GetExtractor()
        {
            var imapServer = await settingStorage.GetValue("general_email_imap_server");
            var imapPort = await settingStorage.GetValue("general_email_imap_port");
            if (string.IsNullOrWhiteSpace(imapPort))
            {
                imapPort = "143";
            }
            var email = await settingStorage.GetValue("general_email_login");
            var password = encryptHelper.Decrypt(await settingStorage.GetValue("general_email_password"));

            ImapConnectionData connectionData = new ImapConnectionData(email, password, imapServer, imapPort);

            var attachmentExtractor = new EmailAttachmentExtractor(connectionData, zipExtractor);
            return attachmentExtractor;
        }
    }
}
