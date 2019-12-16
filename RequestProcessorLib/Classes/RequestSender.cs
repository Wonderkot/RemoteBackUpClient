﻿using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RequestProcessorLib.Interfaces;
using RequestProcessorLib.Util;
using Flurl;

namespace RequestProcessorLib.Classes
{
    public enum ActionList
    {
        CreateNewBackup, CheckExistFile, GetLastBackUp
    }
    public class RequestSender : IRequestSender
    {
        private const string CheckFileMethod = "/CheckFile/";
        private const string GetFileMethod = "/Get/";
        private const string DownloadCompleted = "Download completed";
        private readonly RequestManager _manager;
        private const string ApiUrl = "/api/remoteBackup";

        public RequestSender()
        {
            _manager = new RequestManager();
        }

        private string CreateNewBackupRequest(string url, string dbName)
        {
            ShowMessage?.Invoke("Send request ... ");
            url = url.AppendPathSegment(ApiUrl);
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
                    ShowMessage?.Invoke(DownloadCompleted);
                    ShowBalloonTip?.Invoke(DownloadCompleted);
                    return b64.Result;
                }
            }

            return null;
        }

        public event Action<string> ShowMessage;
        public event Action<string> ShowBalloonTip;

        private string CheckLastBackup(string url, string dbName)
        {
            ShowMessage?.Invoke("Checking last backup ...");
            url = url.AppendPathSegment(ApiUrl).AppendPathSegment(CheckFileMethod).AppendPathSegment(dbName);
            //url = string.Concat(url, CheckFileMethod, dbName);
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
                ShowBalloonTip?.Invoke(jObject.message.ToString());

            }

            return null;
        }

        private string GetLastBackUp(string url, string dbName)
        {
            ShowMessage?.Invoke("Checking for existing backup file...");
            var checkUrl = url.AppendPathSegment(ApiUrl).AppendPathSegment(CheckFileMethod).AppendPathSegment(dbName);
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
                    ShowMessage?.Invoke(DownloadCompleted);
                    ShowBalloonTip?.Invoke(DownloadCompleted);
                    return b64.Result;
                }
                ShowMessage?.Invoke(jObject.message.ToString());
            }

            return null;
        }

        public string InvokeAction(string url, string dbName, ActionList actionList)
        {
            string result = null;
            try
            {
                _manager.GetToken(url);
            }
            catch (Exception e)
            {
                ShowMessage?.Invoke(e.Message);
                return null;
            }
            switch (actionList)
            {
                case ActionList.CreateNewBackup:
                    result = CreateNewBackupRequest(url, dbName);
                    break;
                case ActionList.CheckExistFile:
                    result = CheckLastBackup(url, dbName);
                    break;
                case ActionList.GetLastBackUp:
                    result = GetLastBackUp(url, dbName);
                    break;

            }

            return result;
        }

        public void Init(Action<string> onShowMessage, Action<string> onShowBalloonMsg, string settingsLogin, string settingsPassword)
        {
            ShowMessage = onShowMessage;
            ShowBalloonTip = onShowBalloonMsg;
            _manager.ShowMessage = ShowMessage;
            _manager.Login = settingsLogin;
            _manager.Password = settingsPassword;
            _manager.ResetToken();
        }
    }
}