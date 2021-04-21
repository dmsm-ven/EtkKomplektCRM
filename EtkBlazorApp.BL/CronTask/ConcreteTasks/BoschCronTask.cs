using EtkBlazorApp.BL.Templates.PriceListTemplates;

namespace EtkBlazorApp.BL.CronTask
{
    public class BoschCronTask : CronTaskByPriceListBase
    {
        public BoschCronTask(CronTaskService service) : base(typeof(BoschPriceListTemplate), service, CronTaskPrefix.Prist)
        {

        }
    }
}
