using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AzureSiteReplicator.Startup))]
namespace AzureSiteReplicator
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
