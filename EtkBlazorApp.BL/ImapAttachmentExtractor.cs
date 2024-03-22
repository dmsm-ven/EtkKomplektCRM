using EtkBlazorApp.BL.Data;
using EtkBlazorApp.Core;
using EtkBlazorApp.Core.Interfaces;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class EmailAttachmentExtractor
    {
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        private readonly ICompressedFileExtractor extractor;
        private readonly ImapConnectionData connectionData;

        public EmailAttachmentExtractor(ImapConnectionData connectionData, ICompressedFileExtractor extractor)
        {
            this.connectionData = connectionData;
            this.extractor = extractor;
        }

        public async Task<bool> CheckConnection()
        {
            using (var connection = new MailKit.Net.Imap.ImapClient())
            {
                string userName = connectionData.Email.Split('@')[0];

                await connection.ConnectAsync(connectionData.Host, int.Parse(connectionData.Port), useSsl: true);
                await connection.AuthenticateAsync(new NetworkCredential(connectionData.Email, connectionData.Password));

                return connection.IsConnected;
            }
        }

        public async Task<NewEmailsData> GetPriceListIdsWithNewEmail(
            IReadOnlyDictionary<string, ImapEmailSearchCriteria> criterias, DateTimeOffset? previousLastMessageDateTime, TimeSpan delay)
        {
            var list = new List<string>();
            DateTimeOffset? currentLastMessageDateTime = null;

            using (var connection = new MailKit.Net.Imap.ImapClient())
            {
                connection.Timeout = (int)TimeSpan.FromMinutes(3).TotalMilliseconds;
                connection.AuthenticationMechanisms.Remove("XOAUTH2");
                connection.ServerCertificateValidationCallback = MySslCertificateValidationCallback;
                await connection.ConnectAsync(connectionData.Host, int.Parse(connectionData.Port));
                await connection.AuthenticateAsync(Encoding.UTF8, new NetworkCredential(connectionData.Email, connectionData.Password));
                await connection.Inbox.OpenAsync(FolderAccess.ReadOnly);

                //Делаем проверку что в ящике есть письма
                if (connection.Inbox.Count > 0)
                {
                    //Берем последнее письмо
                    var last = connection.Inbox.GetMessage(connection.Inbox.Count - 1);

                    //Делаем проверку были ли новые письма, с момента последней проверки
                    //либо это первая проверка - тогда ищем письма которые пришли сегодня после 00:00:00
                    if (previousLastMessageDateTime is null || last.Date != previousLastMessageDateTime)
                    {
                        DateTime deliveredAfter = previousLastMessageDateTime is null ?
                            DateTime.Now.Date :
                            DateTime.Now.Subtract(delay);

                        //Ищем письма удовлетворяющие запросу, и добавляем им в список, если такое письмо было найдено
                        foreach (var cr in criterias)
                        {

                            var query = BuildSearchQuery(cr.Value, deliveredAfter);
                            var found = await connection.Inbox.SearchAsync(query);
                            if (found.Count > 0)
                            {
                                nlog.Trace("[+] {sender}", cr.Value.Sender);
                                list.Add(cr.Key);
                            }
                            else
                            {
                                nlog.Trace("[-] {sender}", cr.Value.Sender);
                            }
                        }

                        //Меняем дату последнего сообщения если дошли до этого шага
                        currentLastMessageDateTime = last.Date;
                    }
                }

                await connection.Inbox.CloseAsync();
                await connection.DisconnectAsync(true);
            }

            return new NewEmailsData()
            {
                PriceListIds = list.ToArray(),
                CurrentLastMessageDateTime = currentLastMessageDateTime ?? previousLastMessageDateTime
            };
        }

        public async Task<string> GetLastAttachment(ImapEmailSearchCriteria searchCriteria, DateTime? deliveredAfter = null)
        {
            string fileName = string.Empty;

            using (var connection = new MailKit.Net.Imap.ImapClient())
            {
                connection.Timeout = (int)TimeSpan.FromMinutes(3).TotalMilliseconds;
                connection.AuthenticationMechanisms.Remove("XOAUTH2");
                connection.ServerCertificateValidationCallback = MySslCertificateValidationCallback;
                await connection.ConnectAsync(connectionData.Host, int.Parse(connectionData.Port));
                await connection.AuthenticateAsync(Encoding.UTF8, new NetworkCredential(connectionData.Email, connectionData.Password));
                await connection.Inbox.OpenAsync(FolderAccess.ReadOnly);

                var cap = connection.Capabilities;
                SearchQuery searchQuery = BuildSearchQuery(searchCriteria, deliveredAfter ?? DateTime.Now.Date);
                fileName = await DownloadAttachmentFileWithCriteria(searchCriteria, searchQuery, connection);

                await connection.Inbox.CloseAsync();
                await connection.DisconnectAsync(true);
            }

            return fileName;

        }

        private SearchQuery BuildSearchQuery(ImapEmailSearchCriteria searchCriteria, DateTime deliveredAfter)
        {
            var searchQuery = SearchQuery
                    .FromContains(searchCriteria.Sender)
                    .And(SearchQuery.DeliveredAfter(deliveredAfter));

            if (!string.IsNullOrWhiteSpace(searchCriteria.Subject))
            {
                searchQuery = searchQuery.And(SearchQuery.SubjectContains(searchCriteria.Subject));
            }

            return searchQuery;
        }

        private async Task<string> DownloadAttachmentFileWithCriteria(ImapEmailSearchCriteria searchCriteria, SearchQuery searchQuery, ImapClient connection)
        {
            var searchResult = await connection.Inbox.SearchAsync(searchQuery);

            if (searchResult.Count() == 0)
            {
                nlog.Warn("Письмо с прайс-листом от {sender} с темой {subject} не найдено",
                   searchCriteria?.Sender, searchCriteria?.Subject);

                throw new EmailNotFoundException();
            }
            UniqueId id = searchResult.Max();

            var email = await connection.Inbox.GetMessageAsync(id);
            var attachments = email.Attachments;

            MimePart attachment = null;
            if (attachments.Count() == 1)
            {
                attachment = (MimePart)attachments.First();
            }
            else
            {
                attachment = email.Attachments
                    .Select(i => (MimePart)i)
                    .Where(file => Regex.IsMatch(file.FileName, searchCriteria.FileNamePattern))
                    .FirstOrDefault();
            }
            if (attachment == null)
            {
                throw new ArgumentException("Не найдено вложение с прайс-листом в этом письме");
            }

            var tempPath = Path.Combine(Path.GetTempPath() + attachment.FileName);
            using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                await attachment.Content.DecodeToAsync(fs);
            }

            if (tempPath.EndsWith(".rar") || tempPath.EndsWith(".zip"))
            {
                var files = await extractor.UnzipAll(tempPath, deleteArchive: true);
                return files.FirstOrDefault();
            }
            else
            {
                return tempPath;
            }
        }

        private bool MySslCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

    public class NewEmailsData
    {
        public string[] PriceListIds { get; init; }
        public DateTimeOffset? CurrentLastMessageDateTime { get; init; }
    }
}
