using EtkBlazorApp.DataAccess;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Managers
{
    public class TaskScheduleManager
    {
        private readonly ISettingStorage setting;

        public event Action ProgressStatusChanged;

        private bool _inProgress;
        public bool InProgress
        {
            get => _inProgress;
            set 
            {
                bool stateChanged = value != _inProgress;
                _inProgress = value;
                if (stateChanged)
                {
                    ProgressStatusChanged?.Invoke();
                }
            }
        }

        public DateTime? LastExecution { get; private set; }

        public TaskScheduleManager(ISettingStorage setting)
        {
            this.setting = setting;
        }

        public async Task ExecuteAll()
        {
            InProgress = true;
            LastExecution = null;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await setting.SetValue("task_last_exec_date_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));               
            }
            catch
            {
                throw;
            }
            finally
            {
                LastExecution = DateTime.Now;
                InProgress = false;
                
            }
        }
    }
}
