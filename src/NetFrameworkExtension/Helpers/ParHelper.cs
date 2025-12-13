using MapIdeaHub.BirSign.NetFrameworkExtension.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Helpers
{
    internal class ParHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task EnablePar(
            RedirectToIdentityProviderNotification<
                OpenIdConnectMessage,
                OpenIdConnectAuthenticationOptions> notification)
        {
            // Get PAR Endpoint
            if (BirSignSettings.PushAuthorizationRequestEndpoint == null)
            {
                var openIdConnectConfiguration = await notification.Options.ConfigurationManager.GetConfigurationAsync(default);
                BirSignSettings.PushAuthorizationRequestEndpoint = openIdConnectConfiguration.PushedAuthorizationRequestEndpoint;
            }
            var parEndpoint = BirSignSettings.PushAuthorizationRequestEndpoint;

            // Extract all parameters currently in the message
            var payload = notification.ProtocolMessage.Parameters.ToDictionary(k => k.Key, v => v.Value);

            if (!payload.ContainsKey("client_id"))
            {
                payload.Add("client_id", notification.Options.ClientId);
            }

            if (!payload.ContainsKey("client_secret"))
            {
                payload.Add("client_secret", notification.Options.ClientSecret);
            }

            // Call the PAR Endpoint
            var requestUri = await PushAuthorizationRequestAsync(parEndpoint, payload);

            // Rewrite the Protocol Message
            // Must clear the existing parameters so they aren't sent in the URL
            // However, MUST keep 'client_id' and add 'request_uri'

            // Preserve this
            var clientId = notification.ProtocolMessage.ClientId;

            // Clear sensitive parameters
            notification.ProtocolMessage.Parameters.Clear();

            // Re-add the required public parameters
            notification.ProtocolMessage.ClientId = clientId;
            notification.ProtocolMessage.RequestUri = requestUri;
        }

        public static async Task<string> PushAuthorizationRequestAsync(
            string parEndpoint,
            IDictionary<string, string> parameters)
        {
            var content = new FormUrlEncodedContent(parameters);
            var request = new HttpRequestMessage(HttpMethod.Post, parEndpoint)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"PAR request failed: {response.StatusCode}, Details: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonResponse);

            return json["request_uri"]?.ToString();
        }
    }
}