using MapIdeaHub.BirSign.SharedKernel.Dtos;
using Microsoft.Owin;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MapIdeaHub.BirSign.NetFrameworkExtension
{
    public class WebhookOwinMiddleware : OwinMiddleware
    {
        private readonly string _path;
        private readonly Func<WebhookEvent, Task> _handler;

        public WebhookOwinMiddleware(OwinMiddleware next, string path, Func<WebhookEvent, Task> handler)
            : base(next)
        {
            _path = path;
            _handler = handler;
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Request.Path.Value.Equals(_path, StringComparison.OrdinalIgnoreCase) &&
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

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Success");
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync($"Error handling webhook: {ex.Message}");
                }
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }
}
