using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class SystemEventsLogger
    {
        private readonly ILogStorage logStorage;

        public SystemEventsLogger(ILogStorage logStorage)
        {
            this.logStorage = logStorage;
        }

        public async Task WriteSystemEvent(LogEntryGroupName group, string title, string message)
        {
            var entity = new LogEntryEntity()
            {
                date_time = DateTime.Now,
                user = "System",
                title = title,
                group_name = group.GetDescriptionAttribute(),
                message = message
            };
            await logStorage.Write(entity);
        }
    }
}
