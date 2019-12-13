using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RequestProcessorLib.Interfaces;
using RequestProcessorLib.Util;

namespace RequestProcessorLib.Classes
{
    public class RequestSender : IRequestSender
    {
        private const string CheckFileMethod = "/CheckFile/";
        private const string GetFileMethod = "/Get/";
        private readonly RequestManager _manager;

        public RequestSender()
        {
            _manager = new RequestManager();
        }
        public string CreateNewBackupRequest(string url, string dbName)
        {
            ShowMessage?.Invoke("Send request ... ");

            Task<string> result = _manager.Post(url, dbName);
            ShowMessage?.Invoke(result.Result);

            if (!string.IsNullOrEmpty(result.Result))
            {
                dynamic jObject = JObject.Parse(result.Result);
                if (jObject.Result == 0)
                {
                    var file = jObject.File;
                    ShowMessage?.Invoke("Backup created, start downloading...");
                    Task<string> b64 = _manager.Get(url + GetFileMethod + file);
                    ShowMessage?.Invoke("Download completed");
                    return b64.Result;
                }
            }

            return null;
        }

        public event Action<string> ShowMessage;

        public string CheckLastBackup(string url, string dbName)
        {
            ShowMessage?.Invoke("Checking last backup ...");
            url = string.Concat(url, CheckFileMethod, dbName);
            Task<string> b64 = _manager.Get(url);
            if (!string.IsNullOrEmpty(b64.Result))
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(b64.Result));
                dynamic jObject = JObject.Parse(json);
                if (jObject.result == 0)
                {
                    ShowMessage?.Invoke($"{dbName} created at {jObject.fileInfo}");
                    return null;
                }

                ShowMessage?.Invoke(jObject.message.ToString());
            }

            return null;
        }

        public string GetLastBackUp(string url, string dbName)
        {
            ShowMessage?.Invoke("Checking for existing backup file...");
            var checkUrl = string.Concat(url, CheckFileMethod, dbName);
            Task<string> b64 = _manager.Get(checkUrl);
            if (!string.IsNullOrEmpty(b64.Result))
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(b64.Result));
                dynamic jObject = JObject.Parse(json);
                if (jObject.result == 0)
                {
                    ShowMessage?.Invoke($"Found {dbName} backUp created at {jObject.fileInfo}");
                    var file = jObject.fileName;
                    ShowMessage?.Invoke("Start downloading...");
                    b64 = _manager.Get(url + GetFileMethod + file);
                    ShowMessage?.Invoke("Download completed");
                    return b64.Result;
                }
                ShowMessage?.Invoke(jObject.message.ToString());
            }

            return null;
        }

        public void Init()
        {
            _manager.ShowMessage = ShowMessage;
        }
    }
}