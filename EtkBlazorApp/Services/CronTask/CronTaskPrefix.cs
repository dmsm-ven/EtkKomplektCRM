using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    //Цифровое значение должно соответствовать ID задачи в таблице etk_app_cron_task
    public enum CronTaskPrefix
    {
        Symmetron = 1,
        Silver = 2,
        Prist = 3,
        MarsComponent = 4,
        OdinC = 5,
        Bosch = 6,        
        OzonSeller = 7,
        Megeon = 8
    };
}
