using EtkBlazorApp.DataAccess;
using System.IO;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class EmailAttachmentRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly ISettingStorage settingStorage;
        private readonly ICompressedFileExtractor zipExtractor;
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly string guid;

        internal EmailAttachmentRemoteTemplateFileLoader( 
            ISettingStorage settingStorage,
            ICompressedFileExtractor zipExtractor,
            IPriceListTemplateStorage templateStorage,
            string guid)
        {
            this.settingStorage = settingStorage;
            this.zipExtractor = zipExtractor;
            this.templateStorage = templateStorage;
            this.guid = guid;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            var extractor = await GetExtractor();
            
            string attachmentFilePath = "";
            try
            {
                var templateInfo = await templateStorage.GetPriceListTemplateById(guid);

                ImapEmailSearchCriteria criteria = new ImapEmailSearchCriteria()
                {
                    Subject = templateInfo.email_criteria_subject,
                    Sender = templateInfo.email_criteria_sender,
                    FileNamePattern = templateInfo.email_criteria_file_name_pattern,
                    MaxOldInDays = templateInfo.email_criteria_max_age_in_days
                };

                attachmentFilePath = await extractor.GetLastAttachment(criteria);

                if (attachmentFilePath == null)
                {
                    throw new FileNotFoundException($"Файл по заданным критериям не найден");
                }

                var bytes = File.ReadAllBytes(attachmentFilePath);
                var fileName = Path.GetFileName(attachmentFilePath);

                return new RemoteTemplateFileResponse(bytes, fileName);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (File.Exists(attachmentFilePath))
                {
                    File.Delete(attachmentFilePath);
                }
            }            
        }

        private async Task<EmailAttachmentExtractor> GetExtractor()
        {
            var imapServer = await settingStorage.GetValue("general_email_imap_server");
            var imapPort = await settingStorage.GetValue("general_email_imap_port");
            if (string.IsNullOrWhiteSpace(imapPort))
            {
                imapPort = "143";
            }
            var email = await settingStorage.GetValue("general_email_login");
            var password = EncryptHelper.Decrypt(await settingStorage.GetValue("general_email_password"));

            ImapConnectionData connectionData = new ImapConnectionData(email, password, imapServer, imapPort);

            var attachmentExtractor = new EmailAttachmentExtractor(connectionData, zipExtractor);

            return attachmentExtractor;
        }
    }
}
