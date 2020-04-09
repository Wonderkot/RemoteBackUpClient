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
            foreach (var listItem in _settings.List)
            {
                if (listItem.UseSchedule)
                {
                    var path = Path.Combine(_settings.DefaultPath, listItem.DbName);
                    _requestSender.InvokeAction(listItem.Url, listItem.DbName, path, ActionList.CreateNewBackup);
                }
            }
        }
    }
}