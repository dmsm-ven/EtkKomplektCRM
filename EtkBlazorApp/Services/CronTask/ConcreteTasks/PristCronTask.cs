using EtkBlazorApp.BL;

namespace EtkBlazorApp.Services
{
    public class PristCronTask : CronTaskByPriceListBase
    {
        public PristCronTask(CronTaskService service) : base(typeof(PristPriceListTemplate), service, CronTaskPrefix.Prist)
        {

        }
    }
}
