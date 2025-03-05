using EtkBlazorApp.BL.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using NLog;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Loggers
{
    public class SystemEventsLogger
    {
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

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

            nlog.Trace(" {title} | {group_name} | {message}", entity.title, entity.group_name, entity.message);

            await logStorage.Write(entity);
        }
    }
}
