using EtkBlazorApp.BL;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    public abstract class CronTaskBase
    {
        public CronTaskPrefix Prefix { get; }
        protected CronTaskService service { get; private set; }

        public CronTaskBase(CronTaskService service, CronTaskPrefix prefix)
        {
            this.service = service;
            this.Prefix = prefix;
        }
        
        public abstract Task Run();
    }
}
