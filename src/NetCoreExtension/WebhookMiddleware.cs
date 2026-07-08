using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MapIdeaHub.BirSign.SharedKernel.Dtos;
using Microsoft.AspNetCore.Http;

namespace MapIdeaHub.BirSign.NetCoreExtension
{
    public class WebhookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _path;
        private readonly Func<WebhookEvent, Task> _handler;

        public WebhookMiddleware(RequestDelegate next, string path, Func<WebhookEvent, Task> handler)
        {
            _next = next;
            _path = path;
            _handler = handler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Equals(_path, StringComparison.OrdinalIgnoreCase) &&
                context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        var body = await reader.ReadToEndAsync();
                        var @event = JsonSerializer.Deserialize<WebhookEvent>(body, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (@event != null)
                        {
                            await _handler(@event);
                        }
                    }

                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("Success");
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync($"Error handling webhook: {ex.Message}");
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
