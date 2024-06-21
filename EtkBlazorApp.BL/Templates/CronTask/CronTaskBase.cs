using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.DataAccess.Entity;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.CronTask
{
    public abstract class CronTaskBase
    {
        public int TaskId { get; }
        protected CronTaskService service { get; }

        public CronTaskBase(CronTaskService service, int taskId)
        {
            this.service = service;
            TaskId = taskId;
        }

        public abstract Task Run(CronTaskEntity taskInfo, bool forced);
    }
}
