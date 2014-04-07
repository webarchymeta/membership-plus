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

        public static async Task OnUserConnected(string hubId, string userId, string connectId, string languages)
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
                var cbk = (from d in callbacks where d.HubID == hubId select d).SingleOrDefault();
                if (cbk == null)
                {
                    cbk = new MemberCallback
                    {
                        ApplicationID = AppId,
                        UserID = userId,
                        HubID = hubId,
                        ConnectionID = connectId,
                        IsDisconnected = false,
                        LastActiveDate = DateTime.UtcNow
                    };
                    callbacks.Add(cbk);
                    memb.ChangedMemberCallbacks = callbacks.ToArray();
                }
                else
                {
                    // it is very important to turn this on, otherwise the property will not be marked as modified.
                    // and the service will not save the change!
                    cbk.StartAutoUpdating = true; 
                    cbk.ConnectionID = connectId;
                    cbk.IsDisconnected = false;
                    cbk.LastActiveDate = DateTime.UtcNow;
                    // other more explicit way of doing so is given in the following:
                    //cbk.ConnectionID = connectId;
                    //cbk.IsConnectionIDModified = true;
                    //cbk.IsDisconnected = false;
                    //cbk.IsIsDisconnectedModified = true;
                    //cbk.LastActiveDate = DateTime.UtcNow;
                    //cbk.IsLastActiveDateModified = true;
                }
                await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }

        public static async Task OnUserDisconnected(string hubId, string connectId)
        {
            if (string.IsNullOrEmpty(connectId))
                return;
            var cntx = Cntx;
            MemberCallbackServiceProxy cbksvc = new MemberCallbackServiceProxy();
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "UserID" },
                new QToken { TkName = "asc" }
            });
            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ApplicationID == \"" + AppId + "\" && HubID == \"" + hubId + "\" && ConnectionID == \"" + connectId + "\"" }
            });
            var cbk = (await cbksvc.QueryDatabaseAsync(cntx, new MemberCallbackSet(), qexpr)).FirstOrDefault();
            if (cbk != null)
            {
                // top level entity does not need to set 'StartAutoUpdating' since it is done by the proxy:
                // cbk.StartAutoUpdating = true; 
                cbk.ConnectionID = null;
                cbk.LastActiveDate = DateTime.UtcNow;
                cbk.IsDisconnected = true;
                await cbksvc.AddOrUpdateEntitiesAsync(cntx, new MemberCallbackSet(), new MemberCallback[] { cbk });
            }
        }

        public static async Task UserConnectionClosed(string userId, string languages)
        {
            var mbsvc = new UserAppMemberServiceProxy();
            var cntx = Cntx;
            cntx.AcceptLanguages = languages;
            var memb = await mbsvc.LoadEntityGraphRecursAsync(cntx, AppId, userId, null, null);
            if (memb != null)
            {
                memb.LastActivityDate = DateTime.UtcNow;
                if (memb.ChangedMemberCallbacks!=null)
                {
                    foreach (var c in memb.ChangedMemberCallbacks)
                    {
                        c.StartAutoUpdating = true;
                        c.ConnectionID = null;
                        c.IsDisconnected = true;
                        c.LastActiveDate = DateTime.UtcNow;
                    }
                }
                await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { memb });
            }
        }

        public static Task OnUserReconnected(string hubId, string userId, string connectId, string languages)
        {
            return OnUserConnected(hubId, userId, connectId, languages);
        }
    }
}
