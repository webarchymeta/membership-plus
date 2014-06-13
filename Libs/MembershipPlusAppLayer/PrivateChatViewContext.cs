using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.Security;


namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class PrivateChatViewContext
    {
        internal static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext;
            }
        }

        public static async Task<string[]> GetMessages(string noteHubId, string userId, string set, string qexpr, string prevlast)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(ShortMessage));
            var ser3 = new JavaScriptSerializer();
            System.IO.MemoryStream strm = new System.IO.MemoryStream();
            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
            strm.Write(sbf, 0, sbf.Length);
            strm.Position = 0;
            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
            var svc = new ShortMessageServiceProxy();
            var _set = new ShortMessageSet();
            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
            _set.PageSize_ = int.Parse(sobj["pageSize"]);
            if (!sobj.ContainsKey("setFilter"))
                throw new Exception("The page is not properly parameterized!");
            else
            {
                Func<string, string, int> count = (s, p) =>
                {
                    int _cnt = 0;
                    int i = 0;
                    while ((i = s.IndexOf(p, i)) != -1)
                    {
                        _cnt++;
                        i += p.Length;
                    }
                    return _cnt;
                };
                string filter = sobj["setFilter"];
                if (filter.EndsWith("&& ToID is not null && GroupID is null && ( ToID == \"{0}\" || FromID == \"{0}\" )") && count(filter, "||") == 1)
                {
                    filter = string.Format(filter, userId);
                    _set.SetFilter = filter;
                }
                else
                    throw new Exception("The page is not properly parameterized!");
            }
            ShortMessage _prevlast = null;
            if (!string.IsNullOrEmpty(prevlast))
            {
                strm = new System.IO.MemoryStream();
                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                strm.Write(sbf, 0, sbf.Length);
                strm.Position = 0;
                _prevlast = ser2.ReadObject(strm) as ShortMessage;
            }
            var cntx = Cntx;
            var result = await svc.GetPageItemsAsync(cntx, _set, _qexpr, _prevlast);
            var ar = new List<string>();
            var udic = new Dictionary<string, User>();
            foreach (var e in result)
            {
                User u, p;
                ShortMessage msg;
                if (!udic.TryGetValue(e.FromID, out u) || !udic.TryGetValue(e.ToID, out p))
                {
                    EntitySetType[] excludes = new EntitySetType[]
                    {
                        EntitySetType.UserGroup,
                        EntitySetType.ShortMessageAudience,
                        EntitySetType.ShortMessageAttachment,
                        EntitySetType.ShortMessage
                    };
                    EntitySetRelation[] drills = new EntitySetRelation[]
                    {
                        new EntitySetRelation
                        {
                            SetType = EntitySetType.User,
                            RelatedSets = new EntitySetType[] 
                            {
                                EntitySetType.UserAppMember,
                            }
                        }
                    };
                    msg = await svc.LoadEntityGraphRecursAsync(cntx, e.ID, excludes, drills);
                    u = msg.User_FromID;
                    p = msg.User_ToID;
                    {
                        var member = (from d in u.ChangedUserAppMembers where d.ApplicationID == ApplicationContext.App.ID select d).SingleOrDefault();
                        u.ChangedUserAppMembers = new UserAppMember[] { member };
                        if (member.ChangedMemberCallbacks != null && member.ChangedMemberCallbacks.Length > 0)
                        {
                            var cbk = (from d in member.ChangedMemberCallbacks where d.HubID == noteHubId && d.ChannelID == "System" select d).SingleOrDefault();
                            if (cbk != null)
                                member.ChangedMemberCallbacks = new MemberCallback[] { cbk };
                            else
                                member.ChangedMemberCallbacks = new MemberCallback[] { };
                        }
                    }
                    {
                        var member = (from d in p.ChangedUserAppMembers where d.ApplicationID == ApplicationContext.App.ID select d).SingleOrDefault();
                        p.ChangedUserAppMembers = new UserAppMember[] { member };
                        if (member.ChangedMemberCallbacks != null && member.ChangedMemberCallbacks.Length > 0)
                        {
                            var cbk = (from d in member.ChangedMemberCallbacks where d.HubID == noteHubId && d.ChannelID == "System" select d).SingleOrDefault();
                            if (cbk != null)
                                member.ChangedMemberCallbacks = new MemberCallback[] { cbk };
                            else
                                member.ChangedMemberCallbacks = new MemberCallback[] { };
                        }
                    }
                    if (!udic.ContainsKey(e.FromID))
                        udic.Add(e.FromID, u);
                    if (!udic.ContainsKey(e.ToID))
                        udic.Add(e.ToID, p);
                }
                else
                {
                    msg = e;
                    msg.User_FromID = u;
                    msg.User_ToID = p;
                }
                ar.Add(GetJsonMessage(msg, userId, false));
            }
            return ar.ToArray();
        }

        public static async Task<string> LoadMessageDetails(string noteHubId, string userId, string msgId)
        {
            var cntx = Cntx;
            var gsvc = new UserGroupServiceProxy();
            var svc = new ShortMessageServiceProxy();
            var rmsg = await svc.LoadEntityByKeyAsync(cntx, msgId);
            var msg0 = rmsg;
            while (rmsg.ReplyToID != null)
                rmsg = await svc.LoadEntityByKeyAsync(cntx, rmsg.ReplyToID);
            EntitySetType[] excludes = new EntitySetType[]
                    {
                        EntitySetType.UserGroup,
                        EntitySetType.ShortMessageAudience
                    };
            EntitySetRelation[] drills = new EntitySetRelation[]
                    {
                        new EntitySetRelation
                        {
                            SetType = EntitySetType.User,
                            RelatedSets = new EntitySetType[] 
                            {
                                EntitySetType.UserAppMember,
                            }
                        }
                    };
            var msg = await svc.LoadEntityGraphRecursAsync(cntx, rmsg.ID, excludes, drills);
            var msgstr = GetJsonMessage(msg, userId, true, msgId);
            if (msg0.IsNotReceived == false)
            {
                msg0.IsNotReceived = false;
                await svc.AddOrUpdateEntitiesAsync(cntx, new ShortMessageSet(), new ShortMessage[] { msg0 });
            }
            return msgstr;
        }

        public static string GetJsonUser(User user)
        {
            return GroupChatViewContext.GetJsonUser(user);
        }

        public static string GetJsonMessage(ShortMessage msg, string userId, bool dialog, string matchId = null)
        {
            string json = "{ ";
            json += @"""id"": """ + msg.ID + @""", ";
            json += @"""from"": " + GetJsonUser(msg.User_FromID) + @", ";
            json += @"""to"": " + GetJsonUser(msg.User_ToID) + @", ";
            json += @"""replyToId"": """ + (msg.ReplyToID == null ? "" : msg.ReplyToID) + @""", ";
            json += @"""date"": " + GroupChatContext.getUnixJsonTime(msg.CreatedDate) + @", ";
            json += @"""isOffline"": " + (msg.IsNotReceived.HasValue && msg.IsNotReceived.Value ? "true" : "false") + @", ";
            json += @"""self"": " + (msg.FromID == userId ? "true" : "false") + @", ";
            json += @"""lead"": """ + GroupChatContext.GetLeadText(msg.MsgText).Replace("\"", "\\\"") + @""", ";
            json += @"""text"": """ + msg.MsgText.Replace("\"", "\\\"") + @""", ";
            json += @"""isMatch"": " + (matchId == msg.ID ? "true" : "false") + ", ";
            json += @"""score"": ";
            if (msg.ChangedShortMessageAudiences != null)
            {
                int cnt = 0;
                foreach (var v in msg.ChangedShortMessageAudiences)
                    cnt += v.VoteCount;
                json += cnt;
            }
            else
                json += "0";
            if (dialog)
            {
                json += @", ""replies"": [ ";
                if (msg.ChangedShortMessages != null)
                {
                    string subjson = "";
                    foreach (var r in from d in msg.ChangedShortMessages orderby d.CreatedDate ascending select d)
                        subjson += GetJsonMessage(r, userId, true, matchId) + ", ";
                    json += subjson.TrimEnd(" ,".ToCharArray()) + " ";
                }
                json += "]";
            }
            json += " }";
            return json;
        }
    }
}
