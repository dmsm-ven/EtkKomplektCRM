using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class ScheduleTaskBase
    {
        public CronTask Prefix { get; }
        public bool IsDoneToday { get; protected set; }
        protected ScheduleTaskManager Manager { get; private set; }

        public ScheduleTaskBase(CronTask prefix)
        {
            Prefix = prefix;
        }

        public virtual async Task Execute()
        {
            try
            {
                await Run();
            }
            catch
            {
                throw;
            }

            IsDoneToday = true;
        }

        public void Reset()
        {
            IsDoneToday = false;
        }

        public void SetManager(ScheduleTaskManager manager)
        {
            Manager = manager;
        }

        protected abstract Task Run();

    }
}
