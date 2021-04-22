using EtkBlazorApp.BL;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    public abstract class CronTaskBase
    {
        public CronTaskPrefix Prefix { get; }
        public bool IsDoneToday { get; protected set; }
        protected CronTaskService service { get; private set; }

        public CronTaskBase(CronTaskService service, CronTaskPrefix prefix)
        {
            this.service = service;
            this.Prefix = prefix;
        }

        public async Task Execute()
        {
            try
            {
                IsDoneToday = true;
                await Run();
                
            }
            catch
            {
                throw;
            }
            
        }

        public void Reset()
        {
            IsDoneToday = false;
        }

        protected abstract Task Run();
    }
}
