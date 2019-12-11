using System;

namespace RequestProcessorLib.Interfaces
{
    public interface IRequestSender
    {
        void SendRequest(string url);
        event Action<string> ShowMessage;
    }
}