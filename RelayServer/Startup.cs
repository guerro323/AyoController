using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RelayServer.Startup))]
namespace RelayServer
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
