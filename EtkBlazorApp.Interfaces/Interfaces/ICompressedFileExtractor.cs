namespace EtkBlazorApp.Core.Interfaces;

public interface ICompressedFileExtractor
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>Пулные пути до файлов которые были распакованы из архиваны</returns>
    Task<List<string>> UnzipAll(string achiveFile, bool deleteArchive);
}