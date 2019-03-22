using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SoDauBai.Startup))]
namespace SoDauBai
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
