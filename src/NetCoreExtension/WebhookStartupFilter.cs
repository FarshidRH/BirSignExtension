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

        // Legacy optional secret parameter is kept for public API compatibility and ignored.
        public WebhookStartupFilter(string path, Func<IServiceProvider, WebhookEvent, Task> handler,
            string? secret = null) // Legacy optional parameter retained for source/binary compatibility; ignored.
        {
            _path = path;
            _handler = handler;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<WebhookMiddleware>(_path, _handler, (string?)null);
                next(builder);
            };
        }
    }
}
