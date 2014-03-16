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
    public class MemberViewContext
    {
        internal static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext;
            }
        }

        public static async Task<string> GetMembers(string set, string qexpr, string prevlast)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(User));
            var ser3 = new JavaScriptSerializer();
            System.IO.MemoryStream strm = new System.IO.MemoryStream();
            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
            strm.Write(sbf, 0, sbf.Length);
            strm.Position = 0;
            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
            UserServiceProxy svc = new UserServiceProxy();
            UserSet _set = new UserSet();
            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
            _set.PageSize_ = int.Parse(sobj["pageSize"]);
            if (sobj.ContainsKey("setFilter"))
                _set.SetFilter = sobj["setFilter"];
            User _prevlast = null;
            if (!string.IsNullOrEmpty(prevlast))
            {
                strm = new System.IO.MemoryStream();
                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                strm.Write(sbf, 0, sbf.Length);
                strm.Position = 0;
                _prevlast = ser2.ReadObject(strm) as User;
            }
            var result = await svc.GetPageItemsAsync(Cntx, _set, _qexpr, _prevlast);
            var ar = new List<dynamic>();
            string appId = ApplicationContext.App.ID;
            UsersInRoleServiceProxy uirsvc = new UsersInRoleServiceProxy();
            foreach (var e in result)
            {
                //var membs = svc.MaterializeAllUserAppMembers(Cntx, e);
                //var memb = (from d in membs where d.ApplicationID == appId select d).SingleOrDefault();
                UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
                var cond = new UserAppMemberSetConstraints 
                { 
                    ApplicationIDWrap = new ForeignKeyData<string> { KeyValue = appId }, 
                    UserIDWrap = new ForeignKeyData<string> { KeyValue = e.ID } 
                };
                var memb = (await mbsvc.ConstraintQueryAsync(Cntx, new UserAppMemberSet(), cond, null)).SingleOrDefault();
                ar.Add(new { data = e, member = memb, hasIcon = memb != null && !string.IsNullOrEmpty(memb.IconMime) });
            }
            string json = ser3.Serialize(ar);
            return json;
        }

        public static async Task<dynamic> GetBriefMemberDetails(string id)
        {
            UserServiceProxy usvc = new UserServiceProxy();
            // exclude all 
            EntitySetType[] excludes = new EntitySetType[]
            {
                EntitySetType.Announcement,
                EntitySetType.EventCalendar,
                //EntitySetType.UserAppMember,
                EntitySetType.UserAssociation,
                EntitySetType.UserAssocInvitation,
                EntitySetType.UserGroupMember,
                EntitySetType.UserProfile,
                EntitySetType.UsersInRole,
                EntitySetType.UsersRoleHistory
            };
            var cctx = Cntx;
            var graph = await usvc.LoadEntityGraphRecursAsync(cctx, id, excludes, null);
            var member = (from d in graph.ChangedUserAppMembers where d.ApplicationID == ApplicationContext.App.ID select d).Single();
            var Details = (from d in graph.ChangedUserDetails where d.ApplicationID == ApplicationContext.App.ID select d).FirstOrDefault();
            var Communications = (from d in graph.ChangedCommunications where d.ApplicationID == ApplicationContext.App.ID select d).ToArray();
            dynamic obj = null;
            if (Details != null)
            {
                switch (Details.Gender)
                {
                    case "M":
                        Details.Gender = ResourceUtils.GetString("63889cfb9d3cbe05d1bd2be5cc9953fe", "Male");
                        break;
                    case "F":
                        Details.Gender = ResourceUtils.GetString("b719ce180ec7bd9641fece2f920f4818", "Female");
                        break;
                    default :
                        break;
                }
                obj = new
                {
                    hasDetails = true,
                    details = new
                    {
                        gender = Details.Gender,
                        birthdate = Details.BirthDate,
                        description = Details.Description,
                        website = Details.WebsiteUrl,
                        lastModified = Details.LastModified,
                        hasPhoto = !string.IsNullOrEmpty(Details.PhotoMime)
                    },
                    channels = new List<dynamic>()
                };
            }
            else
            {
                obj = new
                {
                    hasDetails = false,
                    details = new {},
                    channels = new List<dynamic>()
                };
            }
            SortedDictionary<int, List<Communication>> dic = new SortedDictionary<int, List<Communication>>();
            foreach (var c in Communications)
            {
                List<Communication> l;
                if (!dic.TryGetValue(c.TypeID, out l))
                {
                    l = new List<Communication>();
                    dic.Add(c.TypeID, l);
                }
                l.Add(c);
            }
            if (!string.IsNullOrEmpty(member.Email))
            {
                List<Communication> leml;
                if (!dic.TryGetValue(6, out leml) || leml.Count == 0)
                {
                    leml = new List<Communication>();
                    leml.Add(new Communication { AddressInfo = member.Email });
                    dic.Add(6, leml);
                }
                else
                {
                    if (!(from d in leml where d.AddressInfo.ToLower().Trim() == member.Email.ToLower().Trim() select d).Any())
                        leml.Insert(0, new Communication { AddressInfo = member.Email });
                }
            }
            if (Details != null && !string.IsNullOrEmpty(Details.WebsiteUrl))
            {
                List<Communication> leml;
                if (!dic.TryGetValue(9, out leml) || leml.Count == 0)
                {
                    leml = new List<Communication>();
                    leml.Add(new Communication { AddressInfo = Details.WebsiteUrl });
                    dic.Add(9, leml);
                }
                else
                {
                    if (!(from d in leml where d.AddressInfo.ToLower().Trim() == Details.WebsiteUrl.ToLower().Trim() select d).Any())
                        leml.Insert(0, new Communication { AddressInfo = Details.WebsiteUrl });
                }
            }
            foreach (var kvp in dic)
            {
                string label = "";
                switch(kvp.Key)
                {
                    case 1:
                        label = ResourceUtils.GetString("9c9d1674420681239f48d5fa8e181534", "Home Addresses");
                        break;
                    case 2:
                        label = ResourceUtils.GetString("a35ce85c83d755ca36c922000e529b42", "Work Addresses");
                        break;
                    case 3:
                        label = ResourceUtils.GetString("926206a444a6304c5989c7ac50696003", "Daytime Phone Numbers");
                        break;
                    case 4:
                        label = ResourceUtils.GetString("106f87b4378b7aa297f4790c212705cd", "Nighttime Phone Numbers");
                        break;
                    case 5:
                        label = ResourceUtils.GetString("701be5946aa3309654adceb89fef73e1", "Mobile Phone Numbers");
                        break;
                    case 6:
                        label = ResourceUtils.GetString("32948fb18d1ae027b936e2ed205f05b8", "E-Mail Addresses");
                        break;
                    case 7:
                        label = ResourceUtils.GetString("5cf3560c135befe2651d67f4b8d787e6", "Instant Message Addresses");
                        break;
                    case 8:
                        label = ResourceUtils.GetString("b3d34dcab5c7808cbfbf07208c680a87", "Voice Mail Addresses");
                        break;
                    case 9:
                        label = ResourceUtils.GetString("ed3ce4ec38212812ef687c8e69870530", "Website Addresses");
                        break;
                }
                dynamic ch = new
                {
                    name = label,
                    addresses = new List<dynamic>()
                };
                foreach (var c in kvp.Value)
                {
                    ch.addresses.Add(new
                    {
                        address = c.AddressInfo,
                        comment = c.Comment
                    });
                }
                obj.channels.Add(ch);
            }
            return obj;
        }
    }
}
