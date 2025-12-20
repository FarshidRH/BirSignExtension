using MapIdeaHub.BirSign.SharedKernel.Dtos;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.SharedKernel.Services
{
    /// <summary>
    /// Provides methods for sending user and role information to an external identity service using HTTP APIs.
    /// </summary>
    /// <remarks>The IdsService class is intended for integration scenarios where user and role data must be
    /// synchronized with an external identity provider. Instances of this class are typically configured with the
    /// authority and API URIs, as well as client credentials required for authentication.</remarks>
    public class IdsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _authorityUri;
        private readonly string _birSignApiUri;
        private readonly string _clientId;
        private readonly string _clientSecret;

        /// <summary>
        /// Initializes a new instance of the IdsService class using the specified authority URI, BirSign API URI,
        /// client ID, and client secret.
        /// </summary>
        /// <remarks>This constructor creates an internal HttpClient instance for use by the service. If
        /// you need to customize the HttpClient, use the constructor that accepts an HttpClient parameter.</remarks>
        /// <param name="authorityUri">The base URI of the authority service used for authentication. Cannot be null or empty.</param>
        /// <param name="birSignApiUri">The base URI of the BirSign API endpoint. Cannot be null or empty.</param>
        /// <param name="clientId">The client identifier used for authentication with the authority service. Cannot be null or empty.</param>
        /// <param name="clientSecret">The client secret, in plain text, used for authentication with the authority service. Cannot be null or empty.</param>
        public IdsService(
            string authorityUri,
            string birSignApiUri,
            string clientId,
            string clientSecret)
            : this(new HttpClient(), authorityUri, birSignApiUri, clientId, clientSecret)
        { }

        /// <summary>
        /// Initializes a new instance of the IdsService class with the specified HTTP client and configuration
        /// parameters.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance used to send HTTP requests to external services. Must not be null.</param>
        /// <param name="authorityUri">The base URI of the authority service used for authentication. Cannot be null or empty.</param>
        /// <param name="birSignApiUri">The base URI of the BirSign API endpoint. Cannot be null or empty.</param>
        /// <param name="clientId">The client identifier used for authentication with the authority service. Cannot be null or empty.</param>
        /// <param name="clientSecret">The client secret, in plain text, used for authentication with the authority service. Cannot be null or empty.</param>
        public IdsService(
            HttpClient httpClient,
            string authorityUri,
            string birSignApiUri,
            string clientId,
            string clientSecret)
        {
            _httpClient = httpClient;
            _authorityUri = authorityUri;
            _birSignApiUri = birSignApiUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// Sends a user registration request to the remote API asynchronously.
        /// </summary>
        /// <param name="userRequest">The user registration details to be sent. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an ApiReponse object with the
        /// API's response as a string.</returns>
        public async Task<string> SendUsersAsync(UserRequest userRequest)
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageUsersApi/Register";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var jsonContent = JsonSerializer.Serialize(userRequest);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var accessToken = await GetAccessTokenAsync("external_user_registration_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sends a set of role assignments to the remote API asynchronously.
        /// </summary>
        /// <param name="roleRequest">The role assignment request to be sent. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an ApiReponse object with the
        /// API's response as a string.</returns>
        public async Task<ApiReponse<string>> SendRolesAsync(RoleRequest roleRequest)
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageRolesApi/SendRoles";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var jsonContent = JsonSerializer.Serialize(roleRequest);
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