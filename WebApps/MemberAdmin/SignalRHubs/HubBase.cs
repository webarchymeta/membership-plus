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
            get { return HttpContext.Current.User.Identity.GetUserId(); }
        }

        public override async Task OnConnected()
        {
            await base.OnConnected();
            await ConnectionContext.OnUserConnected(UserId, Context.ConnectionId);
        }

        //public override async Task OnDisconnected()
        //{
        //    await ConnectionContext.OnUserDisconnected(UserId);
        //    await base.OnDisconnected();
        //}

        public override async Task OnReconnected()
        {
            await base.OnReconnected();
            await ConnectionContext.OnUserReconnected(UserId, Context.ConnectionId);
        }
    }
}
#endif