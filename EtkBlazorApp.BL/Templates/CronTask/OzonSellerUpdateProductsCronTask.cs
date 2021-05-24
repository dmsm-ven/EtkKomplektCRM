using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    public class OzonSellerUpdateProductsCronTask : CronTaskBase
    {
        private readonly OzonSellerManager ozonManager;

        public OzonSellerUpdateProductsCronTask(CronTaskService service, int taskId, OzonSellerManager ozonManager) 
            : base(service, taskId)
        {
            this.ozonManager = ozonManager;
        }

        public override async Task Run()
        {
            var options = new OzonSellerUpdateOptions() { Only1CQuantity = true };
            await ozonManager.Update(options);
        }
    }
}
