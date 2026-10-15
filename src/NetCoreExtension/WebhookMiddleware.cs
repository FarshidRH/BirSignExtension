using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MapIdeaHub.BirSign.SharedKernel;
using MapIdeaHub.BirSign.SharedKernel.Dtos;
using Microsoft.AspNetCore.Http;

namespace MapIdeaHub.BirSign.NetCoreExtension
{
    public class WebhookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _path;
        private readonly Func<IServiceProvider, WebhookEvent, Task> _handler;
        private readonly string? _secret;

        public WebhookMiddleware(RequestDelegate next, string path,
            Func<IServiceProvider, WebhookEvent, Task> handler, string? secret = null)
        {
            _next = next;
            _path = path;
            _handler = handler;
            _secret = secret;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Equals(_path, StringComparison.OrdinalIgnoreCase) ||
                !context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (context.Request.ContentLength > 1024 * 1024)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                return;
            }

            string body;
            using (var reader = new StreamReader(context.Request.Body))
                body = await reader.ReadToEndAsync();
            if (body.Length > 1024 * 1024)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                return;
            }

            // For existing clients signature verification is opt-in through configuration.
            if (!string.IsNullOrWhiteSpace(_secret) &&
                !WebhookSignature.Verify(_secret, context.Request.Headers["X-BirSign-Timestamp"],
                    context.Request.Headers["X-BirSign-Signature"], body))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            WebhookEvent? @event;
            try
            {
                @event = JsonSerializer.Deserialize<WebhookEvent>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (@event == null) throw new JsonException("No event");
            }
            catch (JsonException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            try
            {
                await _handler(context.RequestServices, @event);
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Success");
            }
            catch (Exception)
            {
                // The sender will retry; do not leak sensitive exception details.
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}
