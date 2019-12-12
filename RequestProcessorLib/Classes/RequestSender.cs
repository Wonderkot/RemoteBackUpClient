using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
        public string CreateNewBackupRequest(string url)
        {
            ShowMessage?.Invoke("Send request ... ");

            Task<string> result = _manager.Post(url, "RioVista");
            ShowMessage?.Invoke(result.Result);

            dynamic x = JObject.Parse(result.Result);
            if (x.Result == 0)
            {
                var file = x.File;
                ShowMessage?.Invoke("Backup created, start downloading...");
                Task<string> b64 = _manager.Get(url + "?fileName=" + file);
                ShowMessage?.Invoke("Download completed");
                return b64.Result;
            }
            return null;
        }

        public event Action<string> ShowMessage;
    }
}