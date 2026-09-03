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
        /// <returns>A task that represents the asynchronous operation. The task result contains an ApiReponse
        /// carrying the API's message on success, or a description of the failure.</returns>
        /// <remarks>
        /// The registration endpoint reports failure differently from the rest of the API: it answers
        /// with an HTTP error status and a problem+json body, where SendRoles answers HTTP 200 with
        /// <see cref="ApiReponse{T}.IsSuccess"/> set to false. Both shapes are normalized here so a
        /// caller only has to read <see cref="ApiReponse{T}.IsSuccess"/>.
        /// </remarks>
        public async Task<ApiReponse<string>> SendUsersAsync(UserRequest userRequest)
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageUsersApi/Register";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var jsonContent = JsonSerializer.Serialize(userRequest);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var accessToken = await GetAccessTokenAsync("external_user_registration_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            return await ReadApiResponseAsync(response);
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
            return await ReadApiResponseAsync(response);
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Turns a response from the user or role endpoints into an <see cref="ApiReponse{T}"/>,
        /// whether it arrived as the API's own envelope or as a problem+json error.
        /// </summary>
        /// <remarks>
        /// <see cref="JsonOptions"/> is not optional here. The API serializes camelCase, and
        /// System.Text.Json matches property names case-sensitively by default, so deserializing
        /// without it produces an envelope whose Error is null and whose IsSuccess keeps its
        /// initializer value of true — a failure that reads as a success.
        /// </remarks>
        private static async Task<ApiReponse<string>> ReadApiResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiReponse<string>
                {
                    Data = null,
                    IsSuccess = false,
                    Error = DescribeFailure(response, content),
                };
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var envelope = JsonSerializer.Deserialize<ApiReponse<string>>(content, JsonOptions);
                    if (envelope != null)
                    {
                        return envelope;
                    }
                }
                catch (JsonException)
                {
                    // Not the API's envelope; fall through and hand the caller the raw body.
                }
            }

            return new ApiReponse<string> { Data = content, IsSuccess = true, Error = null };
        }

        /// <summary>
        /// Builds a one-line description of a failed response, reading problem+json when the body
        /// is one and falling back to the raw body otherwise.
        /// </summary>
        private static string DescribeFailure(HttpResponseMessage response, string content)
        {
            var status = (int)response.StatusCode;

            if (string.IsNullOrWhiteSpace(content))
            {
                return $"{status}: {response.ReasonPhrase}";
            }

            try
            {
                using (var document = JsonDocument.Parse(content))
                {
                    var root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        var parts = new List<string>();

                        JsonElement detail;
                        JsonElement title;
                        if (root.TryGetProperty("detail", out detail) && detail.ValueKind == JsonValueKind.String)
                        {
                            parts.Add(detail.GetString());
                        }
                        else if (root.TryGetProperty("title", out title) && title.ValueKind == JsonValueKind.String)
                        {
                            parts.Add(title.GetString());
                        }

                        JsonElement errors;
                        if (root.TryGetProperty("errors", out errors))
                        {
                            var described = new List<string>();
                            DescribeErrors(errors, null, described);
                            if (described.Count > 0)
                            {
                                parts.Add(string.Join("; ", described));
                            }
                        }

                        if (parts.Count > 0)
                        {
                            return $"{status}: {string.Join(" - ", parts)}";
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON at all; the raw body below is the most useful thing we have.
            }

            return $"{status}: {content}";
        }

        /// <summary>
        /// Flattens the "errors" member of a problem+json body into readable messages.
        /// </summary>
        /// <remarks>
        /// The endpoints put three different things there: a plain string array from the identity
        /// service, an array of IdentityError objects from a failed registration, and the
        /// field-to-messages map ValidationProblem produces. All three are handled.
        /// </remarks>
        private static void DescribeErrors(JsonElement errors, string field, List<string> messages)
        {
            switch (errors.ValueKind)
            {
                case JsonValueKind.String:
                    var text = errors.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        messages.Add(string.IsNullOrEmpty(field) ? text : $"{field}: {text}");
                    }
                    break;

                case JsonValueKind.Array:
                    foreach (var item in errors.EnumerateArray())
                    {
                        DescribeErrors(item, field, messages);
                    }
                    break;

                case JsonValueKind.Object:
                    JsonElement description;
                    if (errors.TryGetProperty("description", out description)
                        && description.ValueKind == JsonValueKind.String)
                    {
                        DescribeErrors(description, field, messages);
                        break;
                    }

                    foreach (var member in errors.EnumerateObject())
                    {
                        DescribeErrors(member.Value, member.Name, messages);
                    }
                    break;
            }
        }

        /// <summary>
        /// Retrieves the department tree structure from the remote API asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiReponse<DepartmentApiDto>> GetDepartmentTreeAsync()
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageChartApi/GetDepartmentTree";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var accessToken = await GetAccessTokenAsync("get_chart_data_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<DepartmentApiDto>>(content, JsonOptions);
        }

        /// <summary>
        /// Retrieves the list of positions by department from the remote API asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiReponse<List<PositionApiDto>>> GetPositionsByDepartmentAsync()
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageChartApi/GetPositionsByDepartment";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var accessToken = await GetAccessTokenAsync("get_chart_data_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<List<PositionApiDto>>>(content, JsonOptions);
        }

        /// <summary>
        /// Retrieves the list of user positions by department from the remote API asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiReponse<List<UserPositionApiDto>>> GetUserPositionsByDepartmentAsync()
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageChartApi/GetUserPositionsByDepartment";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var accessToken = await GetAccessTokenAsync("get_chart_data_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<List<UserPositionApiDto>>>(content, JsonOptions);
        }

        /// <summary>
        /// Retrieves the list of active assignments by department from the remote API asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiReponse<List<UserPositionApiDto>>> GetActiveAssignmentsByDepartmentAsync()
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageChartApi/GetActiveAssignmentsByDepartment";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var accessToken = await GetAccessTokenAsync("get_chart_data_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<List<UserPositionApiDto>>>(content, JsonOptions);
        }

        /// <summary>
        /// Retrieves the department tree structure along with detailed information from the remote API asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiReponse<DepartmentTreeApiDto>> GetDepartmentTreeWithDetailsAsync()
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageChartApi/GetDepartmentTreeWithDetails";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var accessToken = await GetAccessTokenAsync("get_chart_data_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<DepartmentTreeApiDto>>(content, JsonOptions);
        }

        /// <summary>
        /// Retrieves the list of users from parent to leaf asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiReponse<List<UserPositionApiDto>>> GetUsersFromParentToLeafAsync()
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageChartApi/GetUsersFromParentToLeaf";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var accessToken = await GetAccessTokenAsync("get_chart_data_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<List<UserPositionApiDto>>>(content, JsonOptions);
        }

        /// <summary>
        /// Retrieves the list of active users from parent to leaf asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiReponse<List<UserPositionApiDto>>> GetActiveUsersFromParentToLeafAsync()
        {
            var requestUri = $"{_birSignApiUri.TrimEnd('/')}/Api/ManageChartApi/GetActiveUsersFromParentToLeaf";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var accessToken = await GetAccessTokenAsync("get_chart_data_scope");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiReponse<List<UserPositionApiDto>>>(content, JsonOptions);
        }

        public async Task<string> GetAccessTokenAsync(string scope)
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