#if !NO_SIGNALR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.Security;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.MembershipPlus.AppLayer;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using MemberAdminMvc5.Models;

namespace MemberAdminMvc5.SignalRHubs
{
    public class NotificationHub : HubBase
    {
        public override string HubIdentity
        {
            get { return "SystemNotification"; }
        }

        public async Task UserCancelInteraction(string PeerId)
        {
            var langs = Context.Request.Headers["Accept-Language"];
            var status = await PrivateChatContext.UserCancelInteraction(
                                       HubIdentity,
                                       (new PrivateChatHub()).HubIdentity,
                                       PeerId,
                                       UserId,
                                       Context.ConnectionId,
                                       langs);
            if (status != null && status.peerNotifier != null)
            {
                var nhub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                nhub.Clients.Client(status.peerNotifier.ConnectionID).serverNotifications(3, status.noticeMsg, new dynamic[] { });
            }
        }
    }
}
#endif