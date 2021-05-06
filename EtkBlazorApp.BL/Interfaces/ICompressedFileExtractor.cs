using SharpCompress.Archives;
using SharpCompress.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface ICompressedFileExtractor
    {
        /// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>Пулные пути до файлов которые были распакованы из архиваны</returns>
		Task<List<string>> UnzipAll(string achiveFile, bool deleteArchive);
    }

	public class SharpCompressFileExtractor : ICompressedFileExtractor
	{
		public async Task<List<string>> UnzipAll(string achiveFile, bool deleteArchive)
		{
			if (!File.Exists(achiveFile)) { return new List<string>(); }

			string downloadFolder = Path.GetDirectoryName(achiveFile);
			var archive = SharpCompress.Archives.Rar.RarArchive.Open(achiveFile);

			await Task.Run(() =>
			{
				foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
				{
					entry.WriteToDirectory(downloadFolder, new ExtractionOptions());
				}
			});
			archive.Dispose();

			if (deleteArchive)
			{
				File.Delete(achiveFile);
			}

			var entries = archive.Entries.Select(e => Path.Combine(downloadFolder, e.Key)).ToList();
			return entries;
		}
	}
}
