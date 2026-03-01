using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CovenantsWebApp.Startup))]
namespace CovenantsWebApp
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
