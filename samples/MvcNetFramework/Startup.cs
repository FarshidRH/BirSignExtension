using Microsoft.IdentityModel.Logging;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcNetFramework.Startup))]
namespace MvcNetFramework
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            // Without this, IdentityModel errors such as IDX10511 hide the real reason
            // behind "[PII of type '...' is hidden]", which makes them very hard to
            // diagnose. Debug builds only: it puts claim values into exception messages.
            // Add IdentityModelEventSource.LogCompleteSecurityArtifact = true as well if
            // you ever need the raw token dumped -- it is off here because that prints
            // the whole JWT, personal claims included, onto the error page.
            IdentityModelEventSource.ShowPII = true;
#endif

            ConfigureAuth(app);
        }
    }
}
