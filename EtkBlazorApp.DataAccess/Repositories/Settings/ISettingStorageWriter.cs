using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ISettingStorageWriter
    {
        public Task SetValue(string name, string value);       
        public Task SetValue<T>(string name, T value);
        public Task SetValueToDateTimeNow(string name);
    }
}
