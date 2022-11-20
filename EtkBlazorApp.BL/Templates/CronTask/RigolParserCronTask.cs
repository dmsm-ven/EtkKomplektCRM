using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    public class RigolParserCronTask : CronTaskBase
    {
        public RigolParserCronTask(CronTaskService service, int taskId) : base(service, taskId) { }

        public override async Task Run(CronTaskEntity taskInfo)
        {
            //var lines = await service.priceListManager.ReadTemplateLines(templateType, ms, response.FileName);

            //await service.updateManager.UpdatePriceAndStock(lines);

            //taskInfo.last_exec_file_size = response.Bytes.Length;
        }
    }
}
