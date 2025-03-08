using EtkBlazorApp.DataAccess.Entity;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.CronTask
{
    public interface CronTask
    {
        public Task Run(CronTaskEntity taskInfo, bool forced);
    }
}
