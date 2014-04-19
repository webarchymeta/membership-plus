#if !NO_SIGNALR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer;
using Archymeta.Web.MembershipPlus.AppLayer.Models;

namespace MemberAdminMvc5.SignalRHubs
{
    public class PrivateChatHub : Hub
    {
        public string HubIdentity
        {
            get { return "PeerToPeerChatting"; }
        }

        protected string UserId
        {
            get
            {
                var c = Context.Request.GetHttpContext();
                return c.User.Identity.GetUserId();
            }
        }

        public async Task<dynamic> UserConnected(string targetId)
        {
            var langs = Context.Request.Headers["Accept-Language"];
            var status = await PrivateChatContext.UserConnected((new NotificationHub()).HubIdentity, HubIdentity, targetId, UserId, Context.ConnectionId, langs);
            switch(status.status)
            {
                case PeerStatus.Connected:
                    Clients.Client(status.peer.ConnectionID).sendMessage("Hello");
                    return new { status = status.status.ToString(), msg = "" };
                case PeerStatus.Notifiable:
                    {
                        var nhub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                        nhub.Clients.Client(status.peerNotifier.ConnectionID).serverNotifications(3, status.noticeMsg, new dynamic[] { });
                    }
                    return new { status = status.status.ToString(), msg = "" };
                case PeerStatus.MessageSent:
                    return new { status = status.status.ToString(), msg = "waiting ..." };
            }
            return new { status = status.status.ToString(), msg = "not able to connect ..." };
        }

        public override async Task OnDisconnected()
        {
            var cb = await ConnectionContext.OnUserDisconnected(HubIdentity, Context.ConnectionId);
            Clients.Client(cb.ChannelID).userDisConnected(GroupChatContext.GetJsonPeer(cb));
            await base.OnDisconnected();
        }

        public void UserConnectResponse(int respType, string peerId, string connectionId)
        {
            switch (respType)
            {
                case 1:
                    Clients.Client(connectionId).peerConnectResponse(new { status= "Connecting" });
                    break;
            }
        }
    }
}
#endif