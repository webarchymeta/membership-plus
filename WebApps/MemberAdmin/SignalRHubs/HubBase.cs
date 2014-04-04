#if !NO_SIGNALR
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Archymeta.Web.MembershipPlus.AppLayer;

namespace MemberAdminMvc5.SignalRHubs
{
    public class HubBase : Hub
    {
        protected string UserId
        {
            get 
            {
                var c = Context.Request.GetHttpContext();
                return c.User.Identity.GetUserId(); 
            }
        }

        protected HttpContextBase Http
        {
            get
            {
                return Context.Request.GetHttpContext();
            }
        }

        public override async Task OnConnected()
        {
            var langs = Context.Request.Headers["Accept-Language"];
            await ConnectionContext.OnUserConnected(UserId, Context.ConnectionId, langs);
            await base.OnConnected();
        }

        //public override async Task OnDisconnected()
        //{
        //    await base.OnDisconnected();
        //    var langs = Context.Request.Headers["Accept-Language"];
        //    await ConnectionContext.UserConnectionClosed(UserId, langs);
        //}

        public override async Task OnReconnected()
        {
            var langs = Context.Request.Headers["Accept-Language"];
            await ConnectionContext.OnUserReconnected(UserId, Context.ConnectionId, langs);
            await base.OnReconnected();
        }
    }
}
#endif