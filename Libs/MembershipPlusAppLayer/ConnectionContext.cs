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
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.Security;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class ConnectionContext
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

        public const string UserAppMemberKey = "CurrUserAppMember";

        protected static UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();

        public static async Task OnUserConnected(string userId, string connectId, string languages)
        {
            var cntx = Cntx;
            cntx.AcceptLanguages = languages;
            var memb = await mbsvc.LoadEntityByKeyAsync(cntx, AppId, userId);
            if (memb != null)
            {
                memb.StartAutoUpdating = true;
                memb.ConnectionID = connectId;
                memb.LastActivityDate = DateTime.UtcNow;
                memb.AcceptLanguages = languages;
                await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }

        public static async Task OnUserDisconnected(string connectId)
        {
            if (string.IsNullOrEmpty(connectId))
                return;
            var cntx = Cntx;
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "UserID" },
                new QToken { TkName = "asc" }
            });
            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ConnectionID == \"" + connectId + "\"" }
            });
            var memb = (await mbsvc.QueryDatabaseAsync(cntx, new UserAppMemberSet(), qexpr)).FirstOrDefault();
            if (memb != null)
            {
                memb.StartAutoUpdating = true;
                memb.ConnectionID = null;
                memb.LastActivityDate = DateTime.UtcNow;
                await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }

        public static async Task UserConnectionClosed(string userId, string languages)
        {
            var cntx = Cntx;
            cntx.AcceptLanguages = languages;
            var memb = await mbsvc.LoadEntityByKeyAsync(cntx, AppId, userId);
            if (memb != null)
            {
                memb.ConnectionID = null;
                memb.LastActivityDate = DateTime.UtcNow;
                await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }

        public static async Task OnUserReconnected(string userId, string connectId, string languages)
        {
            var cntx = Cntx;
            cntx.AcceptLanguages = languages;
            var memb = await mbsvc.LoadEntityByKeyAsync(cntx, AppId, userId);
            if (memb != null)
            {
                memb.ConnectionID = connectId;
                memb.AcceptLanguages = languages;
                await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }
    }
}
