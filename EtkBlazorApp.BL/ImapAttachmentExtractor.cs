using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
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

		public async Task<string> GetLastAttachment(ImapEmailSearchCriteria searchCriteria)
        {
			//var imapLogger = new ProtocolLogger("imap_extactor.log");//, new MailKit.Net.Imap.ImapClient(imapLogger)
			using (var connection = new MailKit.Net.Imap.ImapClient())
            {
				connection.Timeout = (int)TimeSpan.FromMinutes(3).TotalMilliseconds;
				connection.AuthenticationMechanisms.Remove("XOAUTH2");
				connection.ServerCertificateValidationCallback = MySslCertificateValidationCallback;

				await connection.ConnectAsync(connectionData.Host, int.Parse(connectionData.Port));
				await connection.AuthenticateAsync(Encoding.UTF8, new NetworkCredential(connectionData.Email, connectionData.Password));
				await connection.Inbox.OpenAsync(FolderAccess.ReadOnly);

				var cap = connection.Capabilities;

				SearchQuery searchQuery = BuildSearchQuery(searchCriteria);
				var fileName = await DownloadAttachmentFileWithCriteria(searchQuery, searchCriteria.FileNamePattern, connection);

				await connection.Inbox.CloseAsync();
				await connection.DisconnectAsync(true);

				return fileName;
			}

			throw new NotSupportedException();
        }

		private SearchQuery BuildSearchQuery(ImapEmailSearchCriteria searchCriteria)
        {
			var searchQuery = SearchQuery
					.FromContains(searchCriteria.Sender)					
					.And(SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-searchCriteria.MaxOldInDays).Date));

            if (!string.IsNullOrWhiteSpace(searchCriteria.Subject))
            {
				searchQuery = searchQuery.And(SearchQuery.SubjectContains(searchCriteria.Subject));

			}

            return searchQuery;
		}
		
		private async Task<string> DownloadAttachmentFileWithCriteria(SearchQuery searchQuery, string fileNamePattern, ImapClient connection)
        {
			var searchResult = await connection.Inbox.SearchAsync(searchQuery);

			if (searchResult.Count() == 0)
			{
				throw new Exception("Письмо по заданным характеристикам не найдено");
			}
			var id = searchResult.Max();

			var email = await connection.Inbox.GetMessageAsync(id);
			var attachment = (MimePart)email.Attachments.First();

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
}
