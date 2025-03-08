using EtkBlazorApp.BL.Templates.CronTask;
using EtkBlazorApp.DataAccess.Entity;

namespace EtkBlazorApp.BL.Managers
{
    internal record CronTaskQueueEntry(CronTaskEntity TaskDefinition, CronTask Task);
}