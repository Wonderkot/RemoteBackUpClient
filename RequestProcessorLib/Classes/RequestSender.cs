using System;
using RequestProcessorLib.Interfaces;
using RequestProcessorLib.Util;

namespace RequestProcessorLib.Classes
{
    public class RequestSender : IRequestSender
    {
        private readonly RequestManager _manager;

        public RequestSender()
        {
            _manager = new RequestManager();
        }
        public void SendRequest(string url)
        {
            ShowMessage?.Invoke("Send request ... ");

            var x = _manager.Post(url, "RioVista");

            ShowMessage?.Invoke(x.Result);
        }

        public event Action<string> ShowMessage;
    }
}