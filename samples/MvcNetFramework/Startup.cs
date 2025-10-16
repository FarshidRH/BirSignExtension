using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcNetFramework.Startup))]
namespace MvcNetFramework
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
