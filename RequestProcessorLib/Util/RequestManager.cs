using System;
using System.Collections.Generic;
using System.Net.Http;
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

        /// <summary>
        /// Make an async HTTP POST request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<string> Post(string url, Dictionary<string, string> data, string contentType = "application/json")
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
        private async Task<string> ConstructAndMakeRequest(string url, HttpMethod method, Dictionary<string, string> postData)
        {
            string data;

            // Check the cache first

            // POST or GET
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(3);

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
                    var response = await client.PostAsync(url, new FormUrlEncodedContent(postData));
                    data = await response.Content.ReadAsStringAsync();
                }
                else if (method == HttpMethod.Get)
                {
                    data = await client.GetStringAsync(url);
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