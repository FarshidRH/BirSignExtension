using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Ids
{
    public class LogoutService
    {
        private readonly HttpClient _httpClient;

        public LogoutService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task LogoutAsync(string logoutToken, string logoutUri)
        {
            var postData = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("logout_token", logoutToken)
            });

            var response = await _httpClient.PostAsync(logoutUri, postData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Logout request failed.");
            }
        }
    }
}