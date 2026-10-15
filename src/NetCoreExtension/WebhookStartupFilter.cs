using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using MapIdeaHub.BirSign.SharedKernel.Dtos;

namespace MapIdeaHub.BirSign.NetCoreExtension
{
    public class WebhookStartupFilter : IStartupFilter
    {
        private readonly string _path;
        private readonly Func<IServiceProvider, WebhookEvent, Task> _handler;
        private readonly string? _secret;

        public WebhookStartupFilter(string path, Func<IServiceProvider, WebhookEvent, Task> handler,
            string? secret = null)
        {
            _path = path;
            _handler = handler;
            _secret = secret;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<WebhookMiddleware>(_path, _handler, _secret);
                next(builder);
            };
        }
    }
}
