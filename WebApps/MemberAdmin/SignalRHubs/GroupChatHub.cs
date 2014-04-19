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

namespace MemberAdminMvc5.SignalRHubs
{
    public class GroupChatHub : Hub
    {
        public string HubIdentity
        {
            get { return "GroupChatting"; }
        }

        protected string UserId
        {
            get
            {
                var c = Context.Request.GetHttpContext();
                return c.User.Identity.GetUserId();
            }
        }

        public async Task UserConnected(string groupId)
        {
            var langs = Context.Request.Headers["Accept-Language"];
            var cb = await GroupChatContext.UserConnected(HubIdentity, groupId, UserId, Context.ConnectionId, langs);
            var peers = await GroupChatContext.ListConnectIds(HubIdentity, groupId, true);
            if (peers.Length > 0)
            {
                var cids = (from d in peers select d.ConnectionID).ToArray();
                Clients.Clients(cids).userConnected(GroupChatContext.GetJsonPeer(cb));
            }
        }

        public override async Task OnDisconnected()
        {
            var cb = await ConnectionContext.OnUserDisconnected(HubIdentity, Context.ConnectionId);
            var peers = await GroupChatContext.ListConnectIds(HubIdentity, cb.ChannelID);
            if (peers.Length > 0)
            {
                var cids = (from d in peers select d.ConnectionID).ToArray();
                Clients.Clients(cids).userDisConnected(GroupChatContext.GetJsonPeer(cb));
            }
            await base.OnDisconnected();
        }

        public async Task SendSimpleMessageToAll(string groupId, string replyId, dynamic msgObj)
        {
            var id = Context.Request.User.Identity;
            try
            {
                string msgbody = msgObj.body;
                var msg = await GroupChatContext.AddUserMessage((new NotificationHub()).HubIdentity, HubIdentity, id.GetUserId(), groupId, replyId, msgbody);
                var cids = (from d in msg.peers select d.ConnectionID).ToArray();
                Clients.Clients(cids).messageReceived(id.Name, msg.msg);
                var nhub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                if (nhub != null)
                {
                    var ncids = (from d in msg.callbacks where !(from x in msg.peers where x.UserID == d.UserID select x).Any() select d.ConnectionID).ToArray();
                    if (ncids.Length > 0)
                        nhub.Clients.Clients(ncids).serverNotifications(msg.brief, msg.categ.TypeName);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.sendError(ex.Message);
            }
        }

        public async Task VoteOnMessage(string groupId, string msgId, string userId, int del)
        {
            try
            {
                int score = await GroupChatContext.VoteOnMessage(msgId, userId, del);
                var peers = await GroupChatContext.ListConnectIds(HubIdentity, groupId, false);
                var cids = (from d in peers select d.ConnectionID).ToArray();
                Clients.Clients(cids).onUserVoteMessage(msgId, score);
            }
            catch (Exception ex)
            {
                Clients.Caller.sendError(ex.Message);
            }
        }
    }
}
#endif