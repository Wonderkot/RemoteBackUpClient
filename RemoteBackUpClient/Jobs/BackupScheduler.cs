using System;
using Quartz;
using Quartz.Impl;
using RemoteBackUpClient.Utils;

namespace RemoteBackUpClient.Jobs
{
    public class BackupScheduler
    {
        public static async void Start()
        {
            Logger.Log.Info("--- Запуск планировщика ... ---");
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<BackupJob>()
                .Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity("trigger1", "group1")     // идентифицируем триггер с именем и группой
                .WithDailyTimeIntervalSchedule
                (s =>
                    s.WithIntervalInHours(24)
                        .OnEveryDay()
                        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(5, 0))
                )
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }
}
