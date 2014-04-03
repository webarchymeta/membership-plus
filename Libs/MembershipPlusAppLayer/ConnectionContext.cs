using System;
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

        protected static async Task<UserAppMember> GetMember(string userId)
        {
            // use session, signalr used now is of the same nature at present any way
            UserAppMember memb = HttpContext.Current.Session[UserAppMemberKey] as UserAppMember;
            if (memb == null)
            {
                memb = await mbsvc.LoadEntityByKeyAsync(Cntx, AppId, userId);
                HttpContext.Current.Session[UserAppMemberKey] = memb;
            }
            return memb;
        }

        public static async Task OnUserConnected(string userId, string connectId)
        {
            var memb = await mbsvc.LoadEntityByKeyAsync(Cntx, AppId, userId); // await GetMember(userId);
            if (memb != null)
            {
                memb.StartAutoUpdating = true;
                memb.ConnectionID = connectId;
                memb.LastActivityDate = DateTime.UtcNow;
                memb.AcceptLanguages = HttpContext.Current.Request.Headers["Accept-Language"];
                //HttpContext.Current.Session[UserAppMemberKey] = memb;
                await mbsvc.AddOrUpdateEntitiesAsync(Cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }

        public static async Task UserConnectionClosed(string userId)
        {
            var memb = await mbsvc.LoadEntityByKeyAsync(Cntx, AppId, userId); // await GetMember(userId);
            if (memb != null)
            {
                memb.ConnectionID = null;
                memb.LastActivityDate = DateTime.UtcNow;
                //HttpContext.Current.Session[UserAppMemberKey] = memb;
                await mbsvc.AddOrUpdateEntitiesAsync(Cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }

        public static async Task OnUserReconnected(string userId, string connectId)
        {
            var memb = await mbsvc.LoadEntityByKeyAsync(Cntx, AppId, userId);
            if (memb != null)
            {
                memb.ConnectionID = connectId;
                memb.AcceptLanguages = HttpContext.Current.Request.Headers["Accept-Language"];
                //HttpContext.Current.Session[UserAppMemberKey] = memb;
                await mbsvc.AddOrUpdateEntitiesAsync(Cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }
    }
}
