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
        public int task_type_id { get; set; }
        public string task_type_name { get; set; }
        public string name { get; set; }
        public string linked_price_list_guid { get; set; }
        public string description { get; set; }
        public bool enabled { get; set; }
        public bool archived { get; set; }
        public TimeSpan exec_time { get; set; }
        public DateTime? last_exec_date_time { get; set; }
        public CronTaskExecResult? last_exec_result { get; set; }
        public int? last_exec_file_size { get; set; }
    }
}
