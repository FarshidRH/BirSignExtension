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
        private readonly Func<WebhookEvent, Task> _handler;

        public WebhookStartupFilter(string path, Func<WebhookEvent, Task> handler)
        {
            _path = path;
            _handler = handler;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<WebhookMiddleware>(_path, _handler);
                next(builder);
            };
        }
    }
}
