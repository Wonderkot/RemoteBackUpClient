using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RequestProcessorLib.Util
{
    /// <summary>
    /// Request manager encapsulates asynchronous HTTP GET and POST methods
    /// In conjunction with that, it internally caches requests made in the same minute
    /// </summary>
    public class RequestManager
    {
        private readonly Dictionary<string, string> _customHeaders;

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
        /// <param name="contentType"></param>
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

                // POST method
                if (method == HttpMethod.Post)
                {
                    //var formUrlEncodedContent = new FormUrlEncodedContent(postData);
                    var content = new StringContent(postData, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);
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
                    var response = await client.GetAsync(url);
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
    }
}