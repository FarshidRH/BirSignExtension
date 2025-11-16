using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Services
{
    public class IdsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _authorityUri;
        private readonly string _birSignApiUri;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public IdsService() : this(
            ConfigurationManager.AppSettings["BirSign:Authority"],
            ConfigurationManager.AppSettings["BirSign:ApiUri"],
            ConfigurationManager.AppSettings["BirSign:ClientId"],
            ConfigurationManager.AppSettings["BirSign:ClientSecret"])
        { }

        public IdsService(
            string authorityUri,
            string birSignApiUri,
            string clientId,
            string clientSecret)
            : this(new HttpClient(), authorityUri, birSignApiUri, clientId, clientSecret)
        { }

        public IdsService(
            HttpClient httpClient,
            string authorityUri,
            string birSignApiUri,
            string clientId,
            string clientSecretNotHashed)
        {
            _httpClient = httpClient;
            _authorityUri = authorityUri;
            _birSignApiUri = birSignApiUri;
            _clientId = clientId;
            _clientSecret = clientSecretNotHashed;
        }

        public async Task<ApiReponse<string>> SendUsersAsync(UserRequest users)
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageUsersApi/Register";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var jsonContent = JsonSerializer.Serialize(users);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var accessToken = await GetAccessTokenAsync("send_users_to_ids_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<string>>(content);
        }

        public async Task<ApiReponse<string>> SendRolesAsync(RoleRequest roles)
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageRolesApi/SendRoles";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var jsonContent = JsonSerializer.Serialize(roles);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var accessToken = await GetAccessTokenAsync("send_roles_to_ids_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<string>>(content);
        }

        private async Task<string> GetAccessTokenAsync(string scope)
        {
            var parameters = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("client_id", _clientId ),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", scope)
            });

            var requestUri = $"{_authorityUri.TrimEnd('/')}/connect/token";
            var response = await _httpClient.PostAsync(requestUri, parameters);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStreamAsync();
            return (await JsonObject.ParseAsync(content))["access_token"].ToString();
        }
    }
}