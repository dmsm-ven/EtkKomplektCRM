using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public interface IRemoteTemplateFileLoader
    {
        Task<byte[]> GetBytes(); 
    }
}
