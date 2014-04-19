using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.MembershipPlus.AppLayer.Models;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class PrivateChatContext
    {
        protected static string AppId
        {
            get
            {
                return ApplicationContext.App.ID;
            }
        }

        protected static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext;
            }
        }

        public static async Task<ConnectionStatus> UserConnected(string noticeHubId, string hubId, string peerId, string userId, string connectId, string languages)
        {
            var mbsvc = new UserAppMemberServiceProxy();
            var cntx = Cntx;
            cntx.AcceptLanguages = languages;
            var memb = await mbsvc.LoadEntityGraphRecursAsync(cntx, AppId, userId, null, null);
            if (memb != null)
            {
                memb.StartAutoUpdating = true;
                memb.LastActivityDate = DateTime.UtcNow;
                memb.AcceptLanguages = languages;
                List<MemberCallback> callbacks;
                if (memb.ChangedMemberCallbacks == null)
                    callbacks = new List<MemberCallback>();
                else
                    callbacks = new List<MemberCallback>(memb.ChangedMemberCallbacks);
                var cbk = (from d in callbacks where d.HubID == hubId && d.ChannelID == peerId select d).SingleOrDefault();
                if (cbk == null)
                {
                    cbk = new MemberCallback
                    {
                        ApplicationID = AppId,
                        UserID = userId,
                        HubID = hubId,
                        ChannelID = peerId,
                        ConnectionID = connectId,
                        IsDisconnected = false,
                        LastActiveDate = DateTime.UtcNow
                    };
                }
                else
                {
                    // it is very important to turn this on, otherwise the property will not be marked as modified.
                    // and the service will not save the change!
                    cbk.StartAutoUpdating = true;
                    cbk.ConnectionID = connectId;
                    cbk.IsDisconnected = false;
                    cbk.LastActiveDate = DateTime.UtcNow;
                }
                memb.ChangedMemberCallbacks = new MemberCallback[] { cbk };
                await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
                UserServiceProxy usvc = new UserServiceProxy();
                var u = await usvc.LoadEntityByKeyAsync(cntx, userId);
                memb.UserRef = u;

                var peerMb = await mbsvc.LoadEntityByKeyAsync(cntx, AppId, peerId);

                ConnectionStatus status = new ConnectionStatus();
                status.me = cbk;
                MemberCallbackServiceProxy mbcsvc = new MemberCallbackServiceProxy();
                var peerCb = await mbcsvc.LoadEntityByKeyAsync(cntx, userId, hubId, AppId, peerId);
                if (peerCb == null || peerCb.ConnectionID == null || peerCb.IsDisconnected)
                {
                    MemberNotificationTypeServiceProxy ntsvc = new MemberNotificationTypeServiceProxy();
                    var ntype = await ntsvc.LoadEntityByKeyAsync(cntx, 3);
                    var notifier = await mbcsvc.LoadEntityByKeyAsync(cntx, "System", noticeHubId, AppId, peerId);
                    if (notifier != null && notifier.ConnectionID != null && !notifier.IsDisconnected)
                    {
                        status.peerNotifier = notifier;
                        status.status = PeerStatus.Notifiable;
                    }
                    MemberNotification n = new MemberNotification 
                    {
                        ID = Guid.NewGuid().ToString(),
                        Title = string.Format(ResourceUtils.GetString("20dc5913998d0e9ed01360475e46a0f9", "{0} invites you to chat, is waiting ...", peerMb.AcceptLanguages), u.Username),
                        CreatedDate = DateTime.UtcNow,
                        PriorityLevel = 0,
                        ReadCount = 0,
                        ApplicationID = AppId,
                        TypeID = 3,
                        UserID = peerId
                    };
                    n.NoticeMsg = "{ \"peerId\": \"" + userId + "\", \"connectId\": \"" + cbk.ConnectionID + "\", \"msg\": \"" + n.Title + "\" }";
                    n.IsNoticeDataLoaded = true;
                    MemberNotificationServiceProxy nsvc = new MemberNotificationServiceProxy();
                    var r = await nsvc.AddOrUpdateEntitiesAsync(cntx, new MemberNotificationSet(), new MemberNotification[] { n });
                    status.noticeType = ntype.TypeName;
                    status.noticeMsg = "{ \"peerId\": \"" + userId + "\", \"connectId\": \"" + cbk.ConnectionID + "\", \"msg\": \"" + n.Title + "\" }";
                    status.noticeRecId = r.ChangedEntities[0].UpdatedItem.ID;
                }
                else
                {
                    status.status = PeerStatus.Connected;
                }
                if (peerCb != null)
                    peerCb.UserAppMemberRef = peerMb;
                status.peer = peerCb;

                return status;
            }
            return null;
        }

        public static string GetJsonMessage(ShortMessage msg, string userId, User u, bool dialog)
        {
            string json = "{ ";
            json += @"""id"": """ + msg.ID + @""", ";
            json += @"""from"": """ + msg.User_FromID.Username + @""", ";
            json += @"""fromId"": """ + msg.FromID + @""", ";
            json += @"""to"": """ + u.Username + @""", ";
            json += @"""toId"": """ + u.ID + @""", ";
            json += @"""replyToId"": """ + (msg.ReplyToID == null ? "" : msg.ReplyToID) + @""", ";
            json += @"""date"": " + GroupChatContext.getUnixJsonTime(msg.CreatedDate) + @", ";
            json += @"""self"": " + (msg.FromID == userId ? "true" : "false") + @", ";
            json += @"""text"": """ + msg.MsgText.Replace("\"", "\\\"") + @""", ";
            if (dialog)
            {
                json += @"""replies"": [";
                if (msg.ChangedShortMessages != null)
                {
                    foreach (var r in from d in msg.ChangedShortMessages orderby d.CreatedDate ascending select d)
                        json += GetJsonMessage(r, userId, u, true);
                }
                json += "]";
            }
            json += " }";
            return json;
        }
    }
}
