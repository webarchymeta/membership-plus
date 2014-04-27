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
using Archymeta.Web.Security.Resources;

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

        public async Task<dynamic> UserConnected(string PeerId)
        {
            var langs = Context.Request.Headers["Accept-Language"];
            var status = await PrivateChatContext.UserConnected((new NotificationHub()).HubIdentity, HubIdentity, PeerId, UserId, Context.ConnectionId, langs);
            if (status != null)
            {
                switch (status.status)
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
                        return new { status = status.status.ToString(), msg = "" };
                }
                return new { status = status.status.ToString(), msg = "" };
            }
            else
            {
                return new { status = "???", msg = "Member is not found" };
            }
        }

        public async Task ConnectAck(string peerId)
        {
            var peer = await PrivateChatContext.FindPeer(HubIdentity, Context.Request.User.Identity.GetUserId(), peerId);
            if (peer != null)
                Clients.Client(peer.ConnectionID).onConnectAck();
        }

        public override async Task OnDisconnected()
        {
            var cb = await ConnectionContext.OnUserDisconnected(HubIdentity, Context.ConnectionId);
            if (cb != null)
                Clients.Client(cb.ChannelID).userDisConnected(GroupChatContext.GetJsonPeer(cb));
            await base.OnDisconnected();
        }

        public async Task UserConnectResponse(int respType, string peerId, string connectionId, string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                var peer = await PrivateChatContext.LoadUser(peerId);
                var langs = peer.AcceptLanguages;
                switch (respType)
                {
                    case 2:
                        msg = ResourceUtils.GetString("e5f5f73658abf8cf2c8d4ddc3d0a4466", "Hi, please wait ...", langs);
                        break;
                    case 3:
                        msg = ResourceUtils.GetString("fc65af46674fd2f882b51de00db14b2c", "Sorry, I am busy now ...", langs);
                        break;
                    case 4:
                        msg = ResourceUtils.GetString("a68bbf8eb735652404926659b3284b47", "Sorry, I would rather not talking to you now ...", langs);
                        break;
                }
            }
            Clients.Client(connectionId).peerConnectResponse(new { status = respType, msg = msg, connectId = Context.ConnectionId });
        }

        public async Task SyncRecordState(string peerId, bool record)
        {
            var peer = await PrivateChatContext.FindPeer(HubIdentity, Context.Request.User.Identity.GetUserId(), peerId);
            if (peer != null)
                Clients.Client(peer.ConnectionID).onSyncRecordState(record);
        }

        public async Task SendSimpleMessage(string peerId, string replyId, dynamic msgObj, bool record)
        {
            var id = Context.Request.User.Identity;
            try
            {
                string msgbody = msgObj.body;
                var msg = await PrivateChatContext.AddUserMessage(HubIdentity, id.GetUserId(), peerId, replyId, msgbody, record);
                if (msg.peer != null)
                    Clients.Client(msg.peer.ConnectionID).messageReceived(id.Name, msg.msg);
                Clients.Caller.messageReceived(id.Name, msg.msg);
            }
            catch (Exception ex)
            {
                Clients.Caller.sendError(ex.Message);
            }
        }
    }
}
#endif