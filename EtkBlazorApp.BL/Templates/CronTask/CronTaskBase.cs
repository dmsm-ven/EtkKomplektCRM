using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    public abstract class CronTaskBase
    {
        public int TaskId { get; }
        protected CronTaskService service { get; }

        public CronTaskBase(CronTaskService service, int taskId)
        {
            this.service = service;
            this.TaskId = taskId;
        }
        
        public abstract Task Run(CronTaskEntity taskInfo);
    }
}
