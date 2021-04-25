using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface IRemoteTemplateFileLoader
    {
        Task<RemoteTemplateFileResponse> GetFile(); 
    }

    public class RemoteTemplateFileResponse
    {
        public string FileName { get; }
        public byte[] Bytes { get; }

        public RemoteTemplateFileResponse(byte[] bytes, string fileName)
        {
            Bytes = bytes;
            FileName = fileName;
        }
    }
}
