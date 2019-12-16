using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Newtonsoft.Json.Linq;

namespace RequestProcessorLib.Util
{
    /// <summary>
    /// Request manager encapsulates asynchronous HTTP GET and POST methods
    /// In conjunction with that, it internally caches requests made in the same minute
    /// </summary>
    public class RequestManager
    {
        private readonly Dictionary<string, string> _customHeaders;
        private string Token { get; set; }
        private const string AuthUrl = "/mvc/Public/auth/Logon";

        public RequestManager(Dictionary<string, string> customHeaders = null)
        {
            _customHeaders = customHeaders;
        }

        public Action<string> ShowMessage { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Make an async HTTP POST request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> Post(string url, string data)
        {
            return await ConstructAndMakeRequest(url, HttpMethod.Post, data);
        }

        /// <summary>
        /// Make an async HTTP GET request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> Get(string url)
        {
            return await ConstructAndMakeRequest(url, HttpMethod.Get, null);
        }

        /// <summary>
        /// Build up our request
        /// </summary>
        private async Task<string> ConstructAndMakeRequest(string url, HttpMethod method, string postData)
        {
            string data;


            // POST or GET
            using (var client = new HttpClient())
            {
                // we have custom headers
                if (_customHeaders != null && _customHeaders.Count > 0)
                {
                    // Add all our custom headers to our 
                    foreach (var header in _customHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                if (!string.IsNullOrEmpty(Token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                }

                // POST method
                if (method == HttpMethod.Post)
                {
                    //var formUrlEncodedContent = new FormUrlEncodedContent(postData);
                    var content = new StringContent(postData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.PostAsync(url, content);
                    }
                    catch (Exception e)
                    {
                        ShowMessage?.Invoke(e.Message);
                        return null;
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        ShowMessage?.Invoke(response.ReasonPhrase);
                        data = null;
                    }

                }
                else if (method == HttpMethod.Get)
                {
                    HttpResponseMessage response;
                    try
                    {
                        response = await client.GetAsync(url);
                    }
                    catch (Exception e)
                    {
                        ShowMessage?.Invoke(e.Message);
                        return null;
                    }
                    if (response.IsSuccessStatusCode)

                    {
                        var bytes = response.Content.ReadAsByteArrayAsync();
                        data = Convert.ToBase64String(bytes.Result);
                    }
                    else
                    {
                        ShowMessage?.Invoke(response.ReasonPhrase);
                        data = null;
                    }
                }
                else
                {
                    throw new ArgumentException("Method not supported");
                }
            }

            return data;
        }

        public void GetToken(string s)
        {
            var url = s.AppendPathSegment(AuthUrl);

            dynamic json = new JObject();
            json.username = Login;
            json.password = Password;

            var x = Post(url, json.ToString());

            if (!string.IsNullOrEmpty(x.Result))
            {
                JObject j = JObject.Parse(x.Result);
                Token = j["AuthToken"].ToString();
            }
            else
            {
                throw new Exception("Token is empty, cannot perform any action.");
            }
        }

        public void ResetToken()
        {
            Token = null;
        }
    }
}