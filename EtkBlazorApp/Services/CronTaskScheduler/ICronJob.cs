using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services.CronTaskScheduler;

public interface ICronJob
{
    Task Run(CancellationToken token = default);
}
