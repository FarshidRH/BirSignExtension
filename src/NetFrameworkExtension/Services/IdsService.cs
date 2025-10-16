using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Services
{
    public class IdsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _identityServerUrl;
        private readonly string _apiServerUrl;
        private readonly string _clientId;
        private readonly string _clientSecretNotHashed;

        public IdsService(
            string identityServerUrl,
            string apiServerUrl,
            string clientId,
            string clientSecretNotHashed)
            : this(new HttpClient(), identityServerUrl, apiServerUrl, clientId, clientSecretNotHashed)
        {
        }

        public IdsService(
            HttpClient httpClient,
            string identityServerUrl,
            string apiServerUrl,
            string clientId,
            string clientSecretNotHashed)
        {
            _httpClient = httpClient;
            _identityServerUrl = identityServerUrl;
            _apiServerUrl = apiServerUrl;
            _clientId = clientId;
            _clientSecretNotHashed = clientSecretNotHashed;
        }

        public async Task<ApiReponse<string>> SendRolesAsync(RoleRequest roles)
        {
            var requestUri = $"{_apiServerUrl.TrimEnd('/')}/Api/ManageRolesApi/SendRoles";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var jsonContent = JsonConvert.SerializeObject(roles);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var accessToken = await GetAccessTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiReponse<string>>(content);
        }

        public async Task LogoutAsync(string logoutToken, string logoutUri)
        {
            var postData = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("logout_token", logoutToken),
            });

            var response = await _httpClient.PostAsync(logoutUri, postData);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Logout request failed.");
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var parameters = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("client_id", _clientId ),
                    new KeyValuePair<string, string>("client_secret", _clientSecretNotHashed),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "send_roles_to_ids_scope")
            });

            var requestUri = $"{_identityServerUrl.TrimEnd('/')}/connect/token";
            var response = await _httpClient.PostAsync(requestUri, parameters);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content)["access_token"].ToString();
        }
    }
}