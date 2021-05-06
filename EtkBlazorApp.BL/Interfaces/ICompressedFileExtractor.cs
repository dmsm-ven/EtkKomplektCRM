using SharpCompress.Archives;
using SharpCompress.Common;
using System;
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
		public async Task<List<string>> UnzipAll(string compressedFile, bool deleteArchive)
		{
			if (!File.Exists(compressedFile)) { return new List<string>(); }

			List<string> outputFiles = new List<string>();
			string downloadFolder = Path.GetDirectoryName(compressedFile);

			using (IArchive archive = CreateProcessor(compressedFile))
			{
				await Task.Run(() =>
				{
					foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
					{
						entry.WriteToDirectory(downloadFolder, new ExtractionOptions() { Overwrite = true, ExtractFullPath = false });
						outputFiles.Add(Path.Combine(downloadFolder, Path.GetFileName(entry.Key)));
					}
				});

			}
			
			if (deleteArchive)
			{
				
				File.Delete(compressedFile);
			}

			
			return outputFiles;
		}

		private IArchive CreateProcessor(string compressedFile)
        {
			if (compressedFile.EndsWith(".zip"))
			{
				return SharpCompress.Archives.Zip.ZipArchive.Open(compressedFile);
			}
			if (compressedFile.EndsWith(".rar"))
			{
				return SharpCompress.Archives.Rar.RarArchive.Open(compressedFile);
			}

			throw new NotSupportedException("Данный вид архивов не поддерживается");
		}
	}


}
