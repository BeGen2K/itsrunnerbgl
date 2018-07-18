using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ItsRunnerBgl.Utility
{
    public class ApiRequest
    {
        private HttpClient client;
        private string endpoint;

        public ApiRequest(string endpoint, int userId, string authKey)
        {

            this.endpoint = endpoint;

            var handler = new HttpClientHandler() {UseCookies = true};
            handler.CookieContainer = new CookieContainer();
            handler.CookieContainer.Add(new Uri(endpoint), new Cookie("user", $"{userId}"));
            handler.CookieContainer.Add(new Uri(endpoint), new Cookie("authKey", $"{authKey}"));

            client = new HttpClient(handler);
            client.BaseAddress = new Uri(endpoint);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public async Task<T> GetRequest<T>(string url)
        {
            T model = default(T);
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<T>(str);
            }
            return model;
        }
        public async Task<string> GetRequest(string url)
        {
            var str = "";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                str = await response.Content.ReadAsStringAsync();
            }
            return str;
        }

        public async Task<T> PostRequest<T>(string url, object data)
        {
            T model = default(T);
            var postData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"); // The content type is set here. Request error otherwise.
            HttpResponseMessage response = await client.PostAsync(url, postData);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<T>(str);
            }
            return model;
        }
        public async Task<string> PostRequest(string url, object data)
        {
            var str = "";
            var postData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, postData);
            if (response.IsSuccessStatusCode)
            {
                str = await response.Content.ReadAsStringAsync();
            }
            return str;
        }
    }
}
