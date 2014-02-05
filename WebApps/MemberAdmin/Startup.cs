using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MemberAdminMvc5.Startup))]
namespace MemberAdminMvc5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
