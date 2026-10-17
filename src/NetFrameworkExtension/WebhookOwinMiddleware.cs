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
        public WebhookOwinMiddleware(OwinMiddleware next, string path,
            Func<WebhookEvent, Task> handler) : base(next)
        {
            _path = path;
            _handler = handler;
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!string.Equals(context.Request.Path.Value, _path, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                await Next.Invoke(context);
                return;
            }

            if (context.Request.ContentLength > 1024 * 1024)
            {
                context.Response.StatusCode = 413;
                return;
            }
            string body;
            using (var reader = new StreamReader(context.Request.Body))
                body = await reader.ReadToEndAsync();
            if (body.Length > 1024 * 1024)
            {
                context.Response.StatusCode = 413;
                return;
            }


            WebhookEvent @event;
            try
            {
                @event = JsonSerializer.Deserialize<WebhookEvent>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (@event == null) throw new JsonException("No event");
            }
            catch (JsonException)
            {
                context.Response.StatusCode = 400;
                return;
            }

            try
            {
                await _handler(@event);
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Success");
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
            }
        }
    }
}
