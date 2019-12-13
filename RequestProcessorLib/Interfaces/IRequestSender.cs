using System;

namespace RequestProcessorLib.Interfaces
{
    public interface IRequestSender
    {
        /// <summary>
        /// Create new backup file by request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        string CreateNewBackupRequest(string url, string dbName);
        string CheckLastBackup(string urlTbText, string dbName);
        string GetLastBackUp(string url, string dbName);
        void Init(Action<string> onShowMessage, Action<string> onShowBalloonMsg, string settingsLogin, string settingsPassword);
    }
}