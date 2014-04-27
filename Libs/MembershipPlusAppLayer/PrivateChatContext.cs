using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
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

        protected static int InitMsgTimeWindow
        {
            get
            {
                if (!_initMsgTimeWindow.HasValue)
                {
                    string str = ConfigurationManager.AppSettings["MaxInitialChatMessagesTimeWindowMinutes"] == null ? "" : ConfigurationManager.AppSettings["MaxInitialChatMessagesTimeWindowMinutes"];
                    int ival;
                    if (int.TryParse(str, out ival))
                        _initMsgTimeWindow = ival;
                    else
                        _initMsgTimeWindow = 30;
                }
                return _initMsgTimeWindow.Value;
            }
        }
        protected static int? _initMsgTimeWindow;

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
                    n.NoticeMsg = "{ \"peerId\": \"" + userId + "\", \"peer\": \"" + u.Username + "\", \"connectId\": \"" + connectId + "\", \"msg\": \"" + n.Title + "\", \"isCancel\": false }";
                    n.IsNoticeDataLoaded = true;
                    MemberNotificationServiceProxy nsvc = new MemberNotificationServiceProxy();
                    var r = await nsvc.AddOrUpdateEntitiesAsync(cntx, new MemberNotificationSet(), new MemberNotification[] { n });
                    status.noticeType = ntype.TypeName;
                    status.noticeMsg = "{ \"peerId\": \"" + userId + "\", \"peer\": \"" + u.Username + "\", \"connectId\": \"" + connectId + "\", \"msg\": \"" + n.Title + "\", \"isCancel\": false }";
                    status.noticeRecId = r.ChangedEntities[0].UpdatedItem.ID;
                }
                else
                {
                    UserAssociationServiceProxy uasvc = new UserAssociationServiceProxy();
                    UserAssociation utop = await uasvc.LoadEntityByKeyAsync(cntx, userId, peerId, ApplicationContext.ChatAssocTypeId);
                    DateTime dt = DateTime.UtcNow;
                    if (utop == null)
                    {
                        utop = new UserAssociation
                        {
                            TypeID = ApplicationContext.ChatAssocTypeId,
                            FromUserID = userId,
                            ToUserID = peerId,
                            CreateDate = dt,
                            AssocCount = 1,
                            LastAssoc = dt,
                            InteractCount = 0,
                            Votes = 0
                        };
                    }
                    else
                        utop.AssocCount++;
                    UserAssociation ptou = await uasvc.LoadEntityByKeyAsync(cntx, peerId, userId, ApplicationContext.ChatAssocTypeId);
                    if (ptou == null)
                    {
                        ptou = new UserAssociation
                        {
                            TypeID = ApplicationContext.ChatAssocTypeId,
                            FromUserID = peerId,
                            ToUserID = userId,
                            CreateDate = dt,
                            AssocCount = 1,
                            LastAssoc = dt,
                            InteractCount = 0,
                            Votes = 0
                        };
                    }
                    else
                        ptou.AssocCount++;
                    await uasvc.AddOrUpdateEntitiesAsync(cntx, new UserAssociationSet(), new UserAssociation[] { utop, ptou });
                    status.status = PeerStatus.Connected;
                }
                if (peerCb != null)
                    peerCb.UserAppMemberRef = peerMb;
                status.peer = peerCb;
                return status;
            }
            return null;
        }

        public static async Task<ConnectionStatus> UserCancelInteraction(string noticeHubId, string hubId, string peerId, string userId, string connectId, string languages)
        {
            var cntx = Cntx;
            MemberCallbackServiceProxy mbcsvc = new MemberCallbackServiceProxy();
            ConnectionStatus status = new ConnectionStatus();
            var notifier = await mbcsvc.LoadEntityByKeyAsync(cntx, "System", noticeHubId, AppId, peerId);
            if (notifier != null && notifier.ConnectionID != null && !notifier.IsDisconnected)
            {
                status.peerNotifier = notifier;
                status.status = PeerStatus.Notifiable;
            }
            UserServiceProxy usvc = new UserServiceProxy();
            var u = await usvc.LoadEntityByKeyAsync(cntx, userId);
            var mbsvc = new UserAppMemberServiceProxy();
            var peerMb = await mbsvc.LoadEntityByKeyAsync(cntx, AppId, peerId);
            MemberNotificationTypeServiceProxy ntsvc = new MemberNotificationTypeServiceProxy();
            var ntype = await ntsvc.LoadEntityByKeyAsync(cntx, 3);
            var peerCb = await mbcsvc.LoadEntityByKeyAsync(cntx, userId, hubId, AppId, peerId);
            string title = string.Format(ResourceUtils.GetString("cdc8520b5121c757e6eb79e098d6baef", "{0} cancelled chatting invitation.", peerMb.AcceptLanguages), u.Username);
            if (peerCb == null || peerCb.ConnectionID == null || peerCb.IsDisconnected)
            {
                MemberNotification n = new MemberNotification
                {
                    ID = Guid.NewGuid().ToString(),
                    Title = title,
                    CreatedDate = DateTime.UtcNow,
                    PriorityLevel = 0,
                    ReadCount = 0,
                    ApplicationID = AppId,
                    TypeID = 3,
                    UserID = peerId
                };
                n.NoticeMsg = "{ \"peerId\": \"" + userId + "\", \"peer\": \"" + u.Username + "\", \"connectId\": \"" + connectId + "\", \"msg\": \"" + title + "\", \"isCancel\": true, \"isDisconnect\": false }";
                n.IsNoticeDataLoaded = true;
                MemberNotificationServiceProxy nsvc = new MemberNotificationServiceProxy();
                var r = await nsvc.AddOrUpdateEntitiesAsync(cntx, new MemberNotificationSet(), new MemberNotification[] { n });
                status.noticeType = ntype.TypeName;
                status.noticeMsg = "{ \"peerId\": \"" + userId + "\", \"peer\": \"" + u.Username + "\", \"connectId\": \"" + connectId + "\", \"msg\": \"" + title + "\", \"isCancel\": true, \"isDisconnect\": false }";
                status.noticeRecId = r.ChangedEntities[0].UpdatedItem.ID;
            }
            else
            {
                status.noticeMsg = "{ \"peerId\": \"" + userId + "\", \"peer\": \"" + u.Username + "\", \"connectId\": \"" + connectId + "\", \"msg\": \"" + title + "\", \"isCancel\": true, \"isDisconnect\": true }";
            }
            status.peer = peerCb;
            return status;
        }

        public static async Task<UserAppMember> LoadUser(string userId)
        {
            var cntx = Cntx;
            UserAppMemberServiceProxy msvc = new UserAppMemberServiceProxy();
            var m = await msvc.LoadEntityByKeyAsync(cntx, AppId, userId);
            return m;
        }

        public static async Task<string> LoadUserInfo(string userId, string approot)
        {
            var cntx = Cntx;
            UserAppMemberServiceProxy msvc = new UserAppMemberServiceProxy();
            var m = await msvc.LoadEntityByKeyAsync(cntx, AppId, userId);
            string json = @"{ ""id"": """ + userId + @""", ""icon"": ";
            if (!string.IsNullOrEmpty(m.IconMime))
                json += @"true, ""iconUrl"": """ + approot + @"Account/GetMemberIcon?id=" + userId + @"""";
            else
                json += @"false, ""iconUrl"": """"";
            json += " }";
            return json;
        }

        public static async Task<PeerShotMessage> AddUserMessage(string chatHubId, string userId, string peerId, string replyId, string message, bool record)
        {
            var cntx = Cntx;
            UserServiceProxy usvc = new UserServiceProxy();
            var u = await usvc.LoadEntityByKeyAsync(cntx, userId);
            var peer = await usvc.LoadEntityByKeyAsync(cntx, peerId);
            ShortMessageServiceProxy msvc = new ShortMessageServiceProxy();
            PeerShotMessage m = new PeerShotMessage();
            var now = DateTime.UtcNow;
            ShortMessage msg = new ShortMessage
            {
                ID = Guid.NewGuid().ToString(),
                ApplicationID = AppId,
                TypeID = 1,
                GroupID = null,
                FromID = userId,
                ToID = peerId,
                ReplyToID = string.IsNullOrEmpty(replyId) ? null : replyId,
                CreatedDate = now,
                LastModified = now,
                MsgText = message
            };
            if (record)
            {
                var r = await msvc.AddOrUpdateEntitiesAsync(cntx, new ShortMessageSet(), new ShortMessage[] { msg });
                var _msg = r.ChangedEntities[0].UpdatedItem;
                _msg.User_FromID = u;
                m.msg = GetJsonMessage(_msg, userId, peer, false);
            }
            else
            {
                msg.User_FromID = u;
                m.msg = GetJsonMessage(msg, userId, peer, false);
            }
            UserAssociationServiceProxy uasvc = new UserAssociationServiceProxy();
            var utop = await uasvc.LoadEntityByKeyAsync(cntx, userId, peerId, ApplicationContext.ChatAssocTypeId);
            if (utop != null)
            {
                utop.InteractCount = utop.InteractCount == null ? 1 : utop.InteractCount + 1;
                utop.LastInteract = DateTime.UtcNow;
                await uasvc.AddOrUpdateEntitiesAsync(cntx, new UserAssociationSet(), new UserAssociation[] { utop });
            }
            MembershipPlusServiceProxy svc = new MembershipPlusServiceProxy();
            DateTime dt = DateTime.UtcNow.AddMinutes(-ApplicationContext.OnlineUserInactiveTime);
            MemberCallbackServiceProxy mcbsvc = new MemberCallbackServiceProxy();
            var qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "UserID" },
                new QToken { TkName = "asc" }
            });
            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "HubID == \"" + chatHubId + "\" && ChannelID == \"" + userId + "\" && ConnectionID is not null && IsDisconnected == false" },
                new QToken { TkName = "&&" },
                new QToken { TkName = "ApplicationID == \"" + AppId + "\" && UserID == \"" + peerId + "\"" },
                new QToken { TkName = "&&" },
                new QToken { TkName = "UserAppMemberRef.LastActivityDate > " + svc.FormatRepoDateTime(dt) }
            });
            MemberCallbackServiceProxy cbsv = new MemberCallbackServiceProxy();
            m.peer = (await cbsv.QueryDatabaseAsync(cntx, new MemberCallbackSet(), qexpr)).SingleOrDefault();          
            return m;
        }

        public static async Task<MemberCallback> FindPeer(string chatHubId, string userId, string peerId)
        {
            var cntx = Cntx;
            MembershipPlusServiceProxy svc = new MembershipPlusServiceProxy();
            DateTime dt = DateTime.UtcNow.AddMinutes(-ApplicationContext.OnlineUserInactiveTime);
            MemberCallbackServiceProxy mcbsvc = new MemberCallbackServiceProxy();
            var qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "UserID" },
                new QToken { TkName = "asc" }
            });
            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "HubID == \"" + chatHubId + "\" && ChannelID == \"" + userId + "\" && ConnectionID is not null && IsDisconnected == false" },
                new QToken { TkName = "&&" },
                new QToken { TkName = "ApplicationID == \"" + AppId + "\" && UserID == \"" + peerId + "\"" },
                new QToken { TkName = "&&" },
                new QToken { TkName = "UserAppMemberRef.LastActivityDate > " + svc.FormatRepoDateTime(dt) }
            });
            MemberCallbackServiceProxy cbsv = new MemberCallbackServiceProxy();
            return (await cbsv.QueryDatabaseAsync(cntx, new MemberCallbackSet(), qexpr)).SingleOrDefault();
        }

        public static async Task<string[]> LoadMessages(string peerId, string userId, int maxMessages, bool dialog)
        {
            var svc = new MembershipPlusServiceProxy();
            var usvc = new UserServiceProxy();
            var msgsvc = new ShortMessageServiceProxy();
            var cntx = Cntx;
            var peer = await usvc.LoadEntityByKeyAsync(cntx, peerId);
            DateTime dt = DateTime.UtcNow.AddMinutes(-InitMsgTimeWindow);
            var cond = new ShortMessageSetConstraints
            {
                ApplicationIDWrap = new ForeignKeyData<string> { KeyValue = AppId },
                TypeIDWrap = new ForeignKeyData<int> { KeyValue = 1 },
                GroupIDWrap = new ForeignKeyData<string> { KeyValue = null }
            };
            var qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "CreatedDate" },
                new QToken { TkName = "desc" }
            });
            //                ToIDWrap = new ForeignKeyData<string> { KeyValue = peerId },

            qexpr.FilterTks = new List<QToken>(new QToken[] {
                new QToken { TkName = "( FromID ==  \"" + peerId + "\" && ToID == \"" + userId + "\" || FromID ==  \"" + userId + "\" && ToID == \"" + peerId + "\" ) && CreatedDate > " + svc.FormatRepoDateTime(dt) }
            });
            if (dialog)
            {
                qexpr.FilterTks.Add(new QToken { TkName = " && ReplyToID is null" });
            }
            var msgs = (await msgsvc.ConstraintQueryLimitedAsync(cntx, new ShortMessageSet(), cond, qexpr, maxMessages)).ToArray();
            List<string> jsonMsgs = new List<string>();
            if (msgs.Length > 0)
            {
                for (int i = msgs.Length - 1; i >= 0; i--)
                {
                    EntitySetType[] excludes;
                    if (dialog)
                    {
                        excludes = new EntitySetType[]
                                {
                                    EntitySetType.UserGroup,
                                    //EntitySetType.ShortMessageAudience,
                                    EntitySetType.ShortMessageAttachment
                                };
                    }
                    else
                    {
                        excludes = new EntitySetType[]
                                {
                                    EntitySetType.UserGroup,
                                    //EntitySetType.ShortMessageAudience,
                                    EntitySetType.ShortMessageAttachment,
                                    EntitySetType.ShortMessage
                                };
                    }
                    msgs[i] = await msgsvc.LoadEntityGraphRecursAsync(cntx, msgs[i].ID, excludes, null);
                    jsonMsgs.Add(GetJsonMessage(msgs[i], userId, peer, dialog));
                }
            }
            return jsonMsgs.ToArray();
        }

        public static string GetJsonMessage(ShortMessage msg, string userId, User peer, bool dialog)
        {
            string json = "{ ";
            json += @"""id"": """ + msg.ID + @""", ";
            json += @"""from"": """ + msg.User_FromID.Username + @""", ";
            json += @"""fromId"": """ + msg.FromID + @""", ";
            json += @"""to"": """ + peer.Username + @""", ";
            json += @"""toId"": """ + peer.ID + @""", ";
            json += @"""replyToId"": """ + (msg.ReplyToID == null ? "" : msg.ReplyToID) + @""", ";
            json += @"""date"": " + GroupChatContext.getUnixJsonTime(msg.CreatedDate) + @", ";
            json += @"""self"": false, ";
            json += @"""text"": """ + msg.MsgText.Replace("\"", "\\\"") + @"""";
            if (dialog)
            {
                json += @", ""replies"": [";
                if (msg.ChangedShortMessages != null)
                {
                    string subjson = "";
                    foreach (var r in from d in msg.ChangedShortMessages orderby d.CreatedDate ascending select d)
                        subjson += GetJsonMessage(r, userId, peer, true) + ", ";
                    json += subjson.TrimEnd(" ,".ToCharArray());
                }
                json += "]";
            }
            json += " }";
            return json;
        }
    }
}
