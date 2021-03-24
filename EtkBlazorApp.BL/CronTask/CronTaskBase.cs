using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public abstract class CronTaskBase
    {
        public CronTaskPrefix Prefix { get; }
        public bool IsDoneToday { get; protected set; }
        protected CronTaskManager Manager { get; private set; }

        public CronTaskBase(CronTaskPrefix prefix)
        {
            Prefix = prefix;
        }

        public async Task Execute()
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

        public void SetManager(CronTaskManager manager)
        {
            Manager = manager;
        }

        protected abstract Task Run();

    }
}
