using System;
using RequestProcessorLib.Classes;

namespace RequestProcessorLib.Interfaces
{
    public interface IRequestSender
    {
        string InvokeAction(string url, string dbName, string destination, ActionList actionList);
        void Init(Action<string> onShowMessage, Action<string> onShowBalloonMsg, string settingsLogin, string settingsPassword);
    }
}