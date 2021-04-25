using EtkBlazorApp.BL.Templates.PriceListTemplates;

namespace EtkBlazorApp.BL.CronTask
{
    public class MarsComponentCronTask : CronTaskByPriceListBase
    {
        public MarsComponentCronTask(CronTaskService service) : base(typeof(MarsKomponentPriceListTemplate), service, CronTaskPrefix.MarsComponent)
        {

        }
    }
}
