using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    public class RigolParserCronTask : CronTaskBase
    {
        public RigolParserCronTask(CronTaskService service, int taskId) : base(service, taskId)
        {

        }

        public override Task Run(CronTaskEntity taskInfo)
        {
            throw new NotImplementedException();
        }
    }
}
