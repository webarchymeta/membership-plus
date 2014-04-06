using System;
using System.Configuration;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using Archymeta.Web.MembershipPlus.SignalR;

[assembly: OwinStartupAttribute(typeof(MemberAdminMvc5.Startup))]
namespace MemberAdminMvc5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            if (MemberInitSuccess)
            {
                DataServiceConfiguration config = new DataServiceConfiguration
                {
                    App = App,
                    HostName = Environment.MachineName,
                    TimeWindowInHours = 2,
                    MaxBacklogMessages = 300,
                    MaxQueueLength = 50,
                    HostStateUpdateIntervalInSeconds = 30
                };
                GlobalHost.DependencyResolver.UseDataService(config, ClientContext);
                app.MapSignalR();
            }
        }
    }
}
