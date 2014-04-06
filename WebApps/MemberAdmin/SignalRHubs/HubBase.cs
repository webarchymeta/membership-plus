#if !NO_SIGNALR
using System.Web;
using System.Configuration;
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

        public bool TrackDisconnectState
        {
            get
            {
                if (!_trackDisconnectState.HasValue)
                {
                    if (ConfigurationManager.AppSettings["TrackDisconnectState"] != null)
                    {
                        string bv = ConfigurationManager.AppSettings["TrackDisconnectState"].ToLower();
                        bool b;
                        if (bool.TryParse(bv, out b))
                            _trackDisconnectState = b;
                        else
                            _trackDisconnectState = false;
                    }
                    else
                        _trackDisconnectState = false;
                }
                return _trackDisconnectState.Value;
            }
        }
        private bool? _trackDisconnectState = null;

        public override async Task OnConnected()
        {
            var langs = Context.Request.Headers["Accept-Language"];
            await ConnectionContext.OnUserConnected(UserId, Context.ConnectionId, langs);
            await base.OnConnected();
        }

        public override async Task OnDisconnected()
        {
            await base.OnDisconnected();
            if (TrackDisconnectState)
                await ConnectionContext.OnUserDisconnected(Context.ConnectionId);
        }

        public override async Task OnReconnected()
        {
            var langs = Context.Request.Headers["Accept-Language"];
            await ConnectionContext.OnUserReconnected(UserId, Context.ConnectionId, langs);
            await base.OnReconnected();
        }
    }
}
#endif