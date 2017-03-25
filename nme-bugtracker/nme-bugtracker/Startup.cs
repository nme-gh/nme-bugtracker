using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(nme_bugtracker.Startup))]
namespace nme_bugtracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
