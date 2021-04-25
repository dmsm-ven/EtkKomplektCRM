﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.CronTask
{
    //Цифровое значение должно соответствовать ID задачи в таблице etk_app_cron_task
    //TODO: тут возможно стоит убрать из программы, и перенести на создание напрямую из программы
    public enum CronTaskPrefix
    {
        Symmetron = 1,
        MeanWell_Silver = 2,
        Prist = 3,
        MarsComponent = 4,
        OdinC = 5,
        Bosch = 6,        
        OzonSeller = 7,
        Megeon = 8,
        MeanWell_Partner = 9,
        Testo_Quantity = 10,
    };
}
