using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ISettingStorageReader
    {
        public Task<string> GetValue(string name);
        public Task<T> GetValue<T>(string name);
    }
}
