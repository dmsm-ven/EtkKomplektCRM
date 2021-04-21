using EtkBlazorApp.BL.Templates.PriceListTemplates;

namespace EtkBlazorApp.BL.CronTask
{
    public class MegeonCronTask : CronTaskByPriceListBase
    {
        public MegeonCronTask(CronTaskService service) : base(typeof(MegeonPriceListTemplate), service, CronTaskPrefix.Megeon)
        {

        }
    }
}
