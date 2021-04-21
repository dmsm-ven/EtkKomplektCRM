namespace EtkBlazorApp.BL.CronTask
{
    public class PristCronTask : CronTaskByPriceListBase
    {
        public PristCronTask(CronTaskService service) : base(typeof(PristPriceListTemplate), service, CronTaskPrefix.Prist)
        {

        }
    }
}
