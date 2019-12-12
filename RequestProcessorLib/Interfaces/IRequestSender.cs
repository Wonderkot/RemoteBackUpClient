using System;

namespace RequestProcessorLib.Interfaces
{
    public interface IRequestSender
    {
        string CreateNewBackupRequest(string url);
        event Action<string> ShowMessage;
    }
}