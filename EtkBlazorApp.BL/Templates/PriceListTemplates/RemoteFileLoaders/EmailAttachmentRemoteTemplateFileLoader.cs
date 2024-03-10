using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Repositories.PriceList;
using System.IO;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates.RemoteFileLoaders
{
    public class EmailAttachmentRemoteTemplateFileLoader : IRemoteTemplateFileLoader
    {
        private readonly ISettingStorageReader settingStorage;
        private readonly IPriceListTemplateStorage templateStorage;
        private readonly EmailAttachmentExtractorInitializer extractorInitializer;
        private readonly string guid;

        internal EmailAttachmentRemoteTemplateFileLoader(
            IPriceListTemplateStorage templateStorage,
            EmailAttachmentExtractorInitializer extractorInitializer,
            string guid)
        {
            this.extractorInitializer = extractorInitializer;
            this.templateStorage = templateStorage;
            this.guid = guid;
        }

        public async Task<RemoteTemplateFileResponse> GetFile()
        {
            var extractor = await extractorInitializer.GetExtractor();

            string attachmentFilePath = "";
            try
            {
                var templateInfo = await templateStorage.GetPriceListTemplateById(guid);

                ImapEmailSearchCriteria criteria = new()
                {
                    Subject = templateInfo.email_criteria_subject,
                    Sender = templateInfo.email_criteria_sender,
                    FileNamePattern = templateInfo.email_criteria_file_name_pattern,
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
    }
}
