using System;
using System.IO;
using System.Threading.Tasks;
using Quartz;
using RemoteBackUpClient.Data;
using RemoteBackUpClient.Utils;
using RequestProcessorLib.Classes;
using RequestProcessorLib.Interfaces;

namespace RemoteBackUpClient.Jobs
{
    public class BackupJob : IJob
    {
        private readonly IRequestSender _requestSender = new RequestSender();
        private readonly Settings _settings;
        private event Action<string> ShowMessage;
        private event Action<string> ShowBalloonMsg;

        public BackupJob()
        {
            _settings = SettingsReader.GetSettings();
            var secureString = SettingsReader.DecryptString(_settings.Password);
            var rawPass = SettingsReader.ToInsecureString(secureString);
            //ShowMessage = showMessage;
            //ShowBalloonMsg = showBalloon;

            _requestSender.Init(null, null, _settings.Login, rawPass);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Logger.Log.Info("Выполнение задачи для планировщика.");
            Parallel.ForEach<ListItem>(_settings.List, listItem =>
            {
                if (!listItem.UseSchedule) return;
                var path = Path.Combine(_settings.DefaultPath, listItem.DbName);
                Logger.Log.InfoFormat("БД: {0}", listItem.DbName);
                Logger.Log.InfoFormat("URL: {0}", listItem.Url);
                Logger.Log.InfoFormat("Путь для сохранения: {0}", path);
                _requestSender.InvokeAction(listItem.Url, listItem.DbName, path, ActionList.CreateNewBackup);
            });
            Logger.Log.Info("Задача выполнена.");
        }
    }
}