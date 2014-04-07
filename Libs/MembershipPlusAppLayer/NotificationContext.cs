using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.Security;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class NotificationContext : ConnectionContext
    {
        public static string MapCategoryName(MemberNotificationType type, string acceptLangs = null)
        {
            switch (type.ID)
            {
                case 1:
                    return ResourceUtils.GetString("d8987bcb34881d89920c06a08a5b26f2", "Admin Status Changed", acceptLangs);
                case 2:
                    return ResourceUtils.GetString("a644e8cd597f2b92aa52861632c0363e", "New Messages", acceptLangs);
            }
            return type.TypeName;
        }

        public static async Task<UserAppMember> SetNotification(string userId, SimpleMessage[] msgs)
        {
            UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
            var cntx = Cntx;
            var memb = await mbsvc.LoadEntityByKeyAsync(cntx, AppId, userId);
            if (memb != null)
            {
                memb.ChangedMemberCallbacks = (await mbsvc.MaterializeAllMemberCallbacksAsync(cntx, memb)).ToArray();
                var notices = new List<MemberNotification>();
                foreach (var msg in msgs)
                {
                    notices.Add(new MemberNotification
                    {
                        ID = Guid.NewGuid().ToString(),
                        Title = msg.Title,
                        NoticeMsg = msg.Message,
                        NoticeData = msg.Data,
                        CreatedDate = DateTime.UtcNow,
                        PriorityLevel = (short)msg.Priority,
                        ReadCount = 0,
                        TypeID = msg.TypeId,
                        UserID = userId,
                        ApplicationID = AppId
                    });
                }
                MemberNotificationServiceProxy nsvc = new MemberNotificationServiceProxy();
                var results = await nsvc.AddOrUpdateEntitiesAsync(Cntx, new MemberNotificationSet(), notices.ToArray());
                for (int i = 0; i < msgs.Length; i++)
                    msgs[i].Id = results.ChangedEntities[i].UpdatedItem.ID;
            }
            return memb;
        }

        public static async Task<NotificationTypes> GetTypes()
        {
            string cacheKey = "MemberNotificationTypes";
            NotificationTypes m = HttpContext.Current.Cache[cacheKey] as NotificationTypes;
            if (m == null)
            {
                m = new NotificationTypes();
                MemberNotificationTypeServiceProxy tsvc = new MemberNotificationTypeServiceProxy();
                var categs = await tsvc.QueryDatabaseAsync(Cntx, new MemberNotificationTypeSet(), null);
                List<NotificationType> l = new List<NotificationType>();
                foreach (var c in categs)
                    l.Add(new NotificationType { ID = c.ID, Name = MapCategoryName(c) });
                m.Types = l.ToArray();
                HttpContext.Current.Cache.Add(cacheKey, m, null, DateTime.Now.AddMinutes(30), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
            }
            return m;
        }

        public static async Task<string> GetNotifications(string set, string qexpr, string prevlast)
        {
            var cntx = Cntx;
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(MemberNotification));
            var ser3 = new JavaScriptSerializer();
            System.IO.MemoryStream strm = new System.IO.MemoryStream();
            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
            strm.Write(sbf, 0, sbf.Length);
            strm.Position = 0;
            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
            MemberNotificationServiceProxy svc = new MemberNotificationServiceProxy();
            MemberNotificationSet _set = new MemberNotificationSet();
            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
            _set.PageSize_ = int.Parse(sobj["pageSize"]);
             _set.SetFilter = sobj["setFilter"];
             string usrId = HttpContext.Current.User.Identity.GetUserId();
             if (_set.SetFilter.Contains(usrId))
             {
                 // just in case the script was tempered with by clients.
                 _set.SetFilter += " && UserRef.ID == \"" + usrId + "\"";
             }
            MemberNotification _prevlast = null;
            if (!string.IsNullOrEmpty(prevlast))
            {
                strm = new System.IO.MemoryStream();
                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                strm.Write(sbf, 0, sbf.Length);
                strm.Position = 0;
                _prevlast = ser2.ReadObject(strm) as MemberNotification;
            }
            var result = (await svc.GetPageItemsAsync(cntx, _set, _qexpr, _prevlast)).ToArray();
            List<dynamic> ar = new List<dynamic>();
            Dictionary<int, MemberNotificationType> dic = new Dictionary<int, MemberNotificationType>();
            foreach (var e in result)
            {
                MemberNotificationType c;
                if (!dic.TryGetValue(e.TypeID, out c))
                {
                    c = await svc.MaterializeMemberNotificationTypeRefAsync(cntx, e);
                    c.TypeName = MapCategoryName(c);
                    dic.Add(e.TypeID, c);
                }
                ar.Add(new
                {
                    categ = new { Id = c.ID, name = c.TypeName },
                    data = e
                });
            }
            string json = ser3.Serialize(ar);
            return json;
        }

        public static async Task<List<MemberNotificationType>> GetRecentCategorized(string userId, SimpleMessage[] msgs, int max)
        {
            var cntx = Cntx;
            MembershipPlusServiceProxy svc = new MembershipPlusServiceProxy();
            MemberNotificationTypeServiceProxy tsvc = new MemberNotificationTypeServiceProxy();
            MemberNotificationServiceProxy nsvc = new MemberNotificationServiceProxy();
            var categs = await tsvc.QueryDatabaseAsync(cntx, new MemberNotificationTypeSet(), null);
            List<MemberNotificationType> tlist = new List<MemberNotificationType>();
            DateTime dt = DateTime.UtcNow.AddDays(-1);
            foreach (var categ in categs)
            {
                var cond = new MemberNotificationSetConstraints
                {
                    ApplicationIDWrap = new ForeignKeyData<string> { KeyValue = AppId },
                    UserIDWrap = new ForeignKeyData<string> { KeyValue = userId },
                    TypeIDWrap = new ForeignKeyData<int> { KeyValue = categ.ID }
                };
                QueryExpresion qexpr = new QueryExpresion();
                qexpr.OrderTks = new List<QToken>(new QToken[] { 
                    new QToken { TkName = "PriorityLevel" },
                    new QToken { TkName = "desc" },
                    new QToken { TkName = "CreatedDate" },
                    new QToken { TkName = "desc" }
                });
                qexpr.FilterTks = new List<QToken>(new QToken[] { 
                    new QToken { TkName = "ReadCount == 0 && CreatedDate >= " + svc.FormatRepoDateTime(dt) }
                });
                foreach (var msg in msgs)
                {
                    qexpr.FilterTks.Add(new QToken
                    {
                        TkName = " && ID != \"" + msg.Id + "\""
                    });
                }
                var list = (await nsvc.ConstraintQueryLimitedAsync(cntx, new MemberNotificationSet(), cond, qexpr, max)).ToList();
                if (list.Count > 0)
                {
                    categ.ChangedMemberNotifications = list.ToArray();
                    tlist.Add(categ);
                }
            }
            return tlist;
        }



    }
}
