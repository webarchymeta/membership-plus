using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class ContactContext
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

        public static async Task<UserAssociationType[]> ListTypes()
        {
            UserAssociationTypeServiceProxy tsvc = new UserAssociationTypeServiceProxy();
            return (await tsvc.QueryDatabaseAsync(Cntx, new UserAssociationTypeSet(), null)).ToArray();
        }

        public static async Task<string> GetMembers(string nhubId, string userId, string set, string qexpr, string prevlast, bool outgoing)
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
            var cntx = Cntx;
            var result = await svc.GetPageItemsAsync(cntx, _set, _qexpr, _prevlast);
            var ar = new List<dynamic>();
            string appId = ApplicationContext.App.ID;
            UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
            MemberCallbackServiceProxy cbsvc = new MemberCallbackServiceProxy();
            UserAssociationServiceProxy uasvc = new UserAssociationServiceProxy();
            DateTime dt = DateTime.UtcNow.AddMinutes(-ApplicationContext.OnlineUserInactiveTime);
            foreach (var e in result)
            {
                var cond = new UserAppMemberSetConstraints
                {
                    ApplicationIDWrap = new ForeignKeyData<string> { KeyValue = appId },
                    UserIDWrap = new ForeignKeyData<string> { KeyValue = e.ID }
                };
                var memb = (await mbsvc.ConstraintQueryAsync(cntx, new UserAppMemberSet(), cond, null)).SingleOrDefault();
                bool notify;
                if (outgoing)
                {
                    var notifier = await cbsvc.LoadEntityByKeyAsync(cntx, "System", nhubId, appId, e.ID);
                    notify = memb.LastActivityDate > dt && notifier != null && notifier.ConnectionID != null && !notifier.IsDisconnected;
                }
                else
                    notify = false;
                var cond2 = new UserAssociationSetConstraints();
                if (!outgoing)
                {
                   cond2.FromUserIDWrap = new ForeignKeyData<string> { KeyValue = userId };
                   cond2.ToUserIDWrap = new ForeignKeyData<string> { KeyValue = e.ID };
                   cond2.TypeIDWrap = null;
                }
                else
                {
                    cond2.FromUserIDWrap = new ForeignKeyData<string> { KeyValue = e.ID };
                    cond2.ToUserIDWrap = new ForeignKeyData<string> { KeyValue = userId };
                    cond2.TypeIDWrap = null;
                }
                var assocs = await uasvc.ConstraintQueryAsync(cntx, new UserAssociationSet(), cond2, null);
                var a = new
                {
                    data = e,
                    member = memb,
                    hasIcon = memb != null && !string.IsNullOrEmpty(memb.IconMime),
                    notify = notify,
                    types = new List<int>()
                };
                foreach (var assoc in assocs)
                    a.types.Add(assoc.TypeID);
                ar.Add(a);
            }
            string json = ser3.Serialize(ar);
            return json;
        }
    }
}
