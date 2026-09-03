#if !NET9_0_OR_GREATER
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetCoreExtension.Helpers
{
    /// <summary>
    /// Provides Pushed Authorization Requests (PAR) for target frameworks that predate the
    /// built-in <c>OpenIdConnectOptions.PushedAuthorizationBehavior</c>, which was introduced
    /// in ASP.NET Core 9.0. On .NET 9 and later the built-in support is used instead and this
    /// helper is not compiled.
    /// </summary>
    internal static class ParHelper
    {
        /// <summary>
        /// Pushes the authorization request to the identity provider's PAR endpoint and sends the
        /// browser to the authorization endpoint carrying only <c>client_id</c> and <c>request_uri</c>.
        /// </summary>
        /// <remarks>This mirrors <c>PushedAuthorizationBehavior.UseIfAvailable</c>: when the
        /// provider does not advertise a PAR endpoint, the request is left untouched and the handler
        /// sends a normal front-channel authorization request.
        /// <para>The response is written here rather than letting the handler redirect, because the
        /// handler re-protects <c>state</c> after this event returns. Redirecting ourselves keeps the
        /// pushed <c>state</c> and the one the provider echoes back identical, and keeps every other
        /// parameter out of the front channel as RFC 9126 requires.</para></remarks>
        /// <param name="context">The redirect context for the authorization request.</param>
        public static async Task EnablePar(RedirectContext context)
        {
            var parEndpoint = await GetParEndpointAsync(context);
            if (string.IsNullOrEmpty(parEndpoint))
            {
                return;
            }

            var message = context.ProtocolMessage;

            // The handler only protects 'state' after this event returns, and we are about to take
            // over the response, so build it here. This is the same work the handler would do, and
            // it has to happen before the push so the provider echoes back a state we can unprotect.
            context.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey] = message.RedirectUri;
            message.State = context.Options.StateDataFormat.Protect(context.Properties);

            // Extract all parameters currently in the message, 'state' included.
            var payload = message.Parameters.ToDictionary(k => k.Key, v => v.Value);

            if (!payload.ContainsKey("client_id") && context.Options.ClientId != null)
            {
                payload.Add("client_id", context.Options.ClientId);
            }

            if (!payload.ContainsKey("client_secret") && context.Options.ClientSecret != null)
            {
                payload.Add("client_secret", context.Options.ClientSecret);
            }

            // Call the PAR Endpoint
            var requestUri = await PushAuthorizationRequestAsync(
                context.Options.Backchannel,
                parEndpoint,
                payload,
                context.HttpContext.RequestAborted);

            // Rewrite the Protocol Message
            // Must clear the existing parameters so they aren't sent in the URL
            // However, MUST keep 'client_id' and add 'request_uri'

            // Preserve this
            var clientId = message.ClientId;

            // Clear sensitive parameters
            message.Parameters.Clear();

            // Re-add the required public parameters
            message.ClientId = clientId;
            message.RequestUri = requestUri;

            await SendAuthorizationRequestAsync(context, message);

            // Stop the handler from redirecting again (and from re-protecting 'state').
            context.HandleResponse();
        }

        private static async Task SendAuthorizationRequestAsync(RedirectContext context, OpenIdConnectMessage message)
        {
            if (context.Options.AuthenticationMethod == OpenIdConnectRedirectBehavior.FormPost)
            {
                var buffer = Encoding.UTF8.GetBytes(message.BuildFormPost());
                context.Response.ContentType = "text/html;charset=UTF-8";
                context.Response.ContentLength = buffer.Length;
                context.Response.Headers["Cache-Control"] = "no-cache, no-store";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "Thu, 01 Jan 1970 00:00:00 GMT";
                await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                return;
            }

            context.Response.Redirect(message.CreateAuthenticationRequestUrl());
        }

        private static async Task<string?> GetParEndpointAsync(RedirectContext context)
        {
            var configuration = context.Options.Configuration;
            if (configuration == null && context.Options.ConfigurationManager != null)
            {
                configuration = await context.Options.ConfigurationManager
                    .GetConfigurationAsync(context.HttpContext.RequestAborted);
            }

            return configuration?.PushedAuthorizationRequestEndpoint;
        }

        private static async Task<string> PushAuthorizationRequestAsync(
            HttpClient httpClient,
            string parEndpoint,
            IDictionary<string, string> parameters,
            CancellationToken cancellationToken)
        {
            var content = new FormUrlEncodedContent(parameters);
            var request = new HttpRequestMessage(HttpMethod.Post, parEndpoint)
            {
                Content = content
            };

            var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"PAR request failed: {response.StatusCode}, Details: {responseBody}");
            }

            using (var document = JsonDocument.Parse(responseBody))
            {
                var requestUri = document.RootElement.TryGetProperty("request_uri", out var element)
                    ? element.GetString()
                    : null;

                if (string.IsNullOrEmpty(requestUri))
                {
                    throw new Exception("PAR response did not contain a 'request_uri'.");
                }

                return requestUri;
            }
        }
    }
}
#endif
