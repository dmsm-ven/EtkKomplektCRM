using System;
using System.IO;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public static class StreamExtensions
    {
        public static async Task<MemoryStream> ToMemoryStreamWithProgress(this Stream sourceStream, int bufferSize, int fileSize, IProgress<FileLoadProgress> progress = null)
        {
            progress.Report(FileLoadProgress.Started);

            var bytes = new byte[fileSize];
            var readedBytes = 0;

            var ms = new MemoryStream(bytes);
            while ((readedBytes += await sourceStream.ReadAsync(bytes, readedBytes, Math.Min(bufferSize, fileSize - readedBytes))) < fileSize)
            {
                progress?.Report(new FileLoadProgress(readedBytes, fileSize));
            }
            progress.Report(FileLoadProgress.Finished);
            return ms;

        }
    }

    public struct FileLoadProgress
    {
        public int TotalBytes { get; }
        public int BytesReaded { get; set; }

        public int TotalKb => TotalBytes / 1000;
        public int ReadedKb => BytesReaded / 1000;

        public int Percent => (int)(((double)BytesReaded / TotalBytes) * 100);

        public FileLoadProgress(int readed, int total)
        {
            BytesReaded = readed;
            TotalBytes = total;
        }

        public static FileLoadProgress Finished => new FileLoadProgress(1, 1);
        public static FileLoadProgress Started => new FileLoadProgress(0, 1);
    }
}
