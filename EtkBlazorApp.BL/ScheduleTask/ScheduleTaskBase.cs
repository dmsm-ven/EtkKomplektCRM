using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class ScheduleTaskBase
    {
        public ScheduleTask Name { get; }
        public bool IsDoneToday { get; protected set; }

        public ScheduleTaskBase(ScheduleTask task)
        {
            Name = task;
        }

        public virtual async Task<bool> Execute(ILogStorage logger)
        {
            bool status = false;

            var logEntry = new LogEntryEntity()
            {
                group_name = LogEntryGroupName.PereodicTask,
                message = "Обновления товаров из прайс-листа Symmetron",
                date_time = DateTime.Now
            };

            var sw = Stopwatch.StartNew();

            try
            {
                await Run();

                logEntry.title = $"{Name} выполнено успешно";
                logEntry.message += $" Выполнено за: {sw.Elapsed.TotalSeconds} секунд.";

                status = true;

            }
            catch(Exception ex)
            {
                logEntry.title = $"{Name} ошибка выполнения";
                logEntry.message += $" Ошибка: {ex.Message}";
            }


            IsDoneToday = true;

            await logger.Write(logEntry);
            return status;
        }

        public void Reset()
        {
            IsDoneToday = false;
        }

        protected abstract Task Run();
    }

    public enum ScheduleTask
    {
        Symmetron
    };
}
