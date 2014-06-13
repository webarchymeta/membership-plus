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
    public class GroupChatViewContext
    {
        internal static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext;
            }
        }

        public static async Task<bool> IsUserAMember(string groupId, string userId)
        {
            var svc = new UserGroupMemberServiceProxy();
            return (await svc.LoadEntityByKeyAsync(Cntx, groupId, userId)) != null;
        }

        public static async Task<string[]> UserGroupChatMembers(string userId)
        {
            var svc = new UserGroupMemberServiceProxy();
            var qexpr = new QueryExpresion();
            qexpr.FilterTks = new List<QToken>(new QToken[] {
                new QToken { TkName = "UserID == \"" + userId + "\" && UserGroupRef.GroupTypeID == " + ApplicationContext.ChatGroupTypeID }
            });
            var gl = await svc.QueryDatabaseAsync(Cntx, new UserGroupMemberSet(), qexpr);
            return gl == null ? null : (from d in gl select d.UserGroupID).ToArray();
        }

        public static async Task<UserGroup[]> UserGroupChatGroups(string userId)
        {
            var svc = new UserGroupServiceProxy();
            var qexpr = new QueryExpresion();
            // check this
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ID" },
                new QToken { TkName = "asc" }
            });
            qexpr.FilterTks = new List<QToken>(new QToken[] {
                new QToken { TkName = "GroupTypeID == " + ApplicationContext.ChatGroupTypeID + " && UserGroupMember.UserID == \"" + userId + "\"" }
            });
            var gl = await svc.QueryDatabaseAsync(Cntx, new UserGroupSet(), qexpr);
            return gl == null ? null : (from d in gl select d).ToArray();
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
            Dictionary<string, UserGroup> groupDic = new Dictionary<string, UserGroup>();
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
                if (filter.Contains("ToID is null") && filter.Contains("___usergroups___") && count(filter, "||") == 0)
                {
                    var mbgrps = await UserGroupChatGroups(userId);
                    if (mbgrps == null || mbgrps.Length == 0)
                        throw new Exception(ResourceUtils.GetString("234038e6185f013e25d0213c06f5a0e9", "You are not a member of any chat group."));
                    string groupexpr = "";
                    foreach (var g in mbgrps)
                    {
                        groupexpr += (groupexpr != "" ? " || " : "") + "GroupID == \"" + g.ID + "\"";
                        groupDic.Add(g.ID, g);
                    }
                    _set.SetFilter = filter.Replace("___usergroups___", groupexpr);
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
                User u;
                ShortMessage msg;
                if (!udic.TryGetValue(e.FromID, out u))
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
                    udic.Add(e.FromID, u);
                }
                else
                {
                    msg = e;
                    msg.User_FromID = u;
                }
                ar.Add(GetJsonMessage(msg, userId, groupDic[e.GroupID], false));
            }
            return ar.ToArray();
        }

        public static async Task<string> LoadMessageDetails(string noteHubId, string groupId, string userId, string msgId)
        {
            var cntx = Cntx;
            var gsvc = new UserGroupServiceProxy();
            var svc = new ShortMessageServiceProxy();
            var g = await gsvc.LoadEntityByKeyAsync(cntx, groupId);
            var rmsg = await svc.LoadEntityByKeyAsync(cntx, msgId);
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
            var msgstr = GetJsonMessage(msg, userId, g, true, msgId);
            return msgstr;
        }

        public static string GetJsonUser(User user)
        {
            var member = user.ChangedUserAppMembers[0];
            var cbk = (from d in member.ChangedMemberCallbacks select d).FirstOrDefault();
            bool hasIcon = !string.IsNullOrEmpty(member.IconMime);
            string json = "{ ";
            json += @"""id"": """ + user.ID + @""", ";
            json += @"""name"": """ + user.Username + @""", ";
            json += @"""email"": """ + member.Email + @""", ";
            json += @"""active"": " + (cbk == null || cbk.IsDisconnected || cbk.ConnectionID == null ? "false" : "true") + @", ";
            json += @"""icon"": " + (hasIcon ? "true" : "false") + @", ";
            json += @"""lastActive"": " + (cbk == null ? GroupChatContext.getUnixJsonTime(member.LastActivityDate) : GroupChatContext.getUnixJsonTime(cbk.LastActiveDate)) + @"";
            json += " }";
            return json;
        }

        public static string GetJsonMessage(ShortMessage msg, string userId, UserGroup g, bool dialog, string matchId = null)
        {
            string json = "{ ";
            json += @"""id"": """ + msg.ID + @""", ";
            json += @"""from"": " + GetJsonUser(msg.User_FromID) + @", ";
            json += @"""groupNodes"": [ ";
            string grp = "";
            string path = "";
            foreach (var gn in g.DistinctString.Split('/'))
            {
                if (gn.Trim().Length > 0)
                {
                    path += (path == "" ? "" : "/") + gn;
                    grp += @"{ ""name"": """ + gn + @""", ""path"": """ + path + @""" }, ";
                }
            }
            if (grp != "")
                json += grp.TrimEnd(" ,".ToCharArray()) + " ";
            json +=  "], ";
            json += @"""groupId"": """ + g.ID + @""", ";
            json += @"""replyToId"": """ + (msg.ReplyToID == null ? "" : msg.ReplyToID) + @""", ";
            json += @"""date"": " + GroupChatContext.getUnixJsonTime(msg.CreatedDate) + @", ";
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
                        subjson += GetJsonMessage(r, userId, g, true, matchId) + ", ";
                    json += subjson.TrimEnd(" ,".ToCharArray()) + " ";
                }
                json += "]";
            }
            json += " }";
            return json;
        }
    }
}
