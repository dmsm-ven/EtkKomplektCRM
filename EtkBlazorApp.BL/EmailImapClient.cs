using MailKit;
using MailKit.Search;
using MimeKit;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public static class EmailImapClient
    {
		public static async Task<bool> CheckConnection(string host, string port, string login, string password)
		{
			using (var client = new MailKit.Net.Imap.ImapClient())
			{
				await client.ConnectAsync(host, int.Parse(port), useSsl: true);
				await client.AuthenticateAsync(new NetworkCredential(login, password));
				bool isConnected = client.IsAuthenticated;

				client.Disconnect(true);

				return isConnected;
			}
		}

		/// <summary>
		/// После работы с файлом его необходимо удалить
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="login"></param>
		/// <param name="password"></param>
		/// <returns>Возращает путь до временного файла или в случае неудачи null</returns>
		public static async Task<string> DownloadLastSymmetronPriceListFromMail(string host, string port, string login, string password, uint maxEmailAgeInDays = 5)
        {
			string userName = login.Split('@')[0];

			using (var client = new MailKit.Net.Imap.ImapClient())
			{
				await client.ConnectAsync(host, int.Parse(port), useSsl: true);
				await client.AuthenticateAsync(new NetworkCredential(login, password));
				await client.Inbox.OpenAsync(FolderAccess.ReadOnly);

				var searchQuery = SearchQuery.SubjectContains("Price Symmetron")
					.And(SearchQuery.FromContains("price@symmetron.ru"))
					.And(SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-maxEmailAgeInDays)));

				var searchResult = await client.Inbox.SearchAsync(searchQuery);

				if (searchResult.Any()) 
				{
					var id = searchResult
						.Where(item => Regex.IsMatch(client.Inbox.GetMessage(item).Attachments.First().ContentDisposition?.FileName, @"\d+\.rar"))
						.OrderByDescending(item => item.Id)
						.FirstOrDefault();

					if (id != default)
					{
						var email = await client.Inbox.GetMessageAsync(id);
						var attachment = email.Attachments.First();
						var fileName = await SaveAttachmentFile(attachment);

						return fileName;
					}
				}

				client.Disconnect(true);
			}

			return null;
		}

		private static async Task<string> SaveAttachmentFile(MimeEntity attachment)
		{
			var tempPath = Path.GetTempFileName();
			var downloadFolder = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			downloadFolder = Path.Combine(downloadFolder, "Downloads");

			using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
			{
				await ((MimePart)attachment).Content.DecodeToAsync(fs);
			}

			if (File.Exists(tempPath))
			{
				var archive = SharpCompress.Archives.Rar.RarArchive.Open(tempPath);
				foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
				{
					entry.WriteToDirectory(downloadFolder, new ExtractionOptions());
				}
				archive.Dispose();

				File.Delete(tempPath);

				string filePath = Path.Combine(downloadFolder, archive.Entries.First().Key);
				return filePath;
			}

			return null;
		}				
    }
}
