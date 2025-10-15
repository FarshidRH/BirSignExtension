using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Ids
{
    public static class IdsRoleService
    {
        public static async Task<string> GetAccessTokenAsync(string IdentityServerUrl, string clientId, string clientHashedSecret)
        {
            var client = new HttpClient();
            var parameters = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("client_id", clientId ),
                    new KeyValuePair<string, string>("client_secret", clientHashedSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "send_roles_to_ids_scope")
            });
            var response = await client.PostAsync(IdentityServerUrl.TrimEnd('/') + "/connect/token", parameters);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(content)["access_token"].ToString();
            return "Bearer " + token;
        }

        public static async Task<ApiReponseDto<string>> SendRoleService(string IdentityServerUrl, string ApiServerUrl, string clientId, string clientHashedSecret, List<ApiGetRoleDto> roles)
        {
            var postService = new PostService<ApiReponseDto<string>>();
            var header = new Dictionary<string, string>()
            {
                { "Authorization", await GetAccessTokenAsync(IdentityServerUrl , clientId , clientHashedSecret ) }
            };
            return postService.Post(ApiServerUrl.TrimEnd('/') + "/Api/ManageRolesApi/SendRoles", header, roles);
        }
    }
}