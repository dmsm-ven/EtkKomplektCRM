using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class CronTaskEntity
    {
        public int task_id { get; set; }
        public string name { get; set; }
        public string details_page { get; set; }
        public bool enabled { get; set; }
        public TimeSpan exec_time { get; set; }
        public DateTime last_exec_date_time { get; set; }
    }
}
