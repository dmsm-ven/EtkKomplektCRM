using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MyDbLogger
    {
        private readonly ILogStorage logStorage;

        public MyDbLogger(ILogStorage logStorage)
        {
            this.logStorage = logStorage;
        }

        public async Task Write(LogEntryGroupName group, string user, string title, string message)
        {
            await logStorage.Write(new LogEntryEntity() { date_time = DateTime.Now, user = user, title = title, group_name = group, message = message });
        }
    }
}
