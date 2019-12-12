using System;

namespace RequestProcessorLib.Interfaces
{
    public interface IRequestSender
    {
        string CreateNewBackupRequest(string url, string dbName);
        event Action<string> ShowMessage;
        string CheckLastBackup(string urlTbText, string dbName);
        string GetLastBackUp(string url, string dbName);
    }
}