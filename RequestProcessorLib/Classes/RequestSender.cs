using System.Collections.Generic;
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
            var x = _manager.Post(url, new Dictionary<string, string>() { ["dbName"] = "RioVista" }, contentType: "Custom");
        }

    }
}