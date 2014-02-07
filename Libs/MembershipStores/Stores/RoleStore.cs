using System;
using System.Text;
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
#if MemberPlus
using CryptoGateway.RDB.Data.MembershipPlus;
#else
using CryptoGateway.RDB.Data.AspNetMember;
#endif
using Archymeta.Web.Security.Resources;

namespace Archymeta.Web.Security
{
    public class RoleStore<TRole> : IRoleStore<TRole> where TRole : Role, IApplicationRole, new()
    {
#if TEST
        public CallContext Cctx
        {
            get { return _cctx; }
        }
#endif
        private CallContext _cctx;
        private Application_ app;

        private bool WriteExceptionsToEventLog
        {
            get
            {
                if (_writeExceptionsToEventLog.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["WriteAuthExceptionsToEventLog"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _writeExceptionsToEventLog = bv;
                    else
                        _writeExceptionsToEventLog = false;
                }
                return _writeExceptionsToEventLog.Value;
            }
        }
        private bool? _writeExceptionsToEventLog = default(bool?);

        private bool ThrowOnPopulatedRole
        {
            get
            {
                if (_throwOnPopulatedRole.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["ThrowOnDeletePopulatedRole"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _throwOnPopulatedRole = bv;
                    else
                        _throwOnPopulatedRole = true;
                }
                return _throwOnPopulatedRole.Value;
            }
        }
        private bool? _throwOnPopulatedRole = default(bool?);

        private Exception getException(string id, string err, Exception e = null)
        {
#if MemberPlus
            return new Exception(ResourceUtils.GetString(id, err), e);
#else
            return new Exception(err, e);
#endif
        }

        public RoleStore(CallContext clientContext, Application_ app)
        {
            _cctx = clientContext;
            this.app = app;
        }

        public async Task CreateAsync(TRole role)
        {
            CallContext cctx = _cctx.CreateCopy();
            RoleServiceProxy rsvc = new RoleServiceProxy();
            try
            {
                Role last = null;
                string rolename = role.Name;
                var find = await findRoleAsync(rolename);
                if (find == null || find.Item1 == null)
                {
                    int lev = -1;
                    string[] rolepath = rolename.Trim('.').Split('.');
                    if (find != null && find.Item2 != null)
                    {
                        lev = 0;
                        var x = find.Item2;
                        while (x.UpperRef != null)
                        {
                            lev++;
                            x = x.UpperRef;
                        }
                    }
                    RoleSet rs = new RoleSet();
                    last = (find == null || find.Item2 == null) ? null : find.Item2;
                    for (int i = lev + 1; i < rolepath.Length; i++)
                    {
                        Role r = new Role();
                        r.ApplicationID = app.ID;
                        r.RoleName = rolepath[i];
                        r.DisplayName = rolepath[i];
                        r.Description = "";
                        r.ParentID = last == null ? default(int?) : last.ID;
                        var _r = await rsvc.AddOrUpdateEntitiesAsync(cctx, rs, new Role[] { r });
                        r = _r.ChangedEntities[0].UpdatedItem;
                        last = r;
                    }
                    role.UpdateInstance(last);
                }
                else
                    role.UpdateInstance(find.Item1);
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "CreateRole");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
            }
        }

        public async Task DeleteAsync(TRole role)
        {
            CallContext cctx = _cctx.CreateCopy();
            RoleServiceProxy rsvc = new RoleServiceProxy();
            try
            {
                string rolename = role.Name;
                var find = await findRoleAsync(rolename);
                Role r = find == null ? null : find.Item1;
                if (r != null)
                {
                    if (!ThrowOnPopulatedRole)
                        rsvc.DeleteEntities(cctx, new RoleSet(), new Role[] { r });
                    else
                    {
                        var rus = await GetUsersInRoleAsync(rolename);
                        if (rus == null || rus.Length == 0)
                            rsvc.DeleteEntities(cctx, new RoleSet(), new Role[] { r });
                        else
                            throw getException("6de8c40a93b3d0c36fb4b5daa73d7db5", "Cannot delete a populated role.");
                    }
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteRole");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
            }
        }

        public async Task<TRole> FindByIdAsync(string roleId)
        {
            CallContext cctx = _cctx.CreateCopy();
            RoleServiceProxy rsvc = new RoleServiceProxy();
            var r = await rsvc.LoadEntityByKeyAsync(cctx, int.Parse(roleId));
            if (r == null)
                return null;
            TRole role = new TRole();
            role.UpdateInstance(r);
            role.Name = await rolePathAsync(r);
            return role;
        }

        public async Task<TRole> FindByNameAsync(string roleName)
        {
            var find = await findRoleAsync(roleName);
            if (find == null || find.Item1 == null)
                return null;
            TRole role = new TRole();
            role.UpdateInstance(find.Item1);
            role.Name = roleName;
            return role;
        }

        public async Task UpdateAsync(TRole role)
        {
            var find = await findRoleAsync(role.Name);
            if (find == null || find.Item1 == null)
                return;
            Role r = find.Item1;
            int cnt = 0;
            if (r.RoleName != (role as Role).RoleName)
            {
                r.RoleName = (role as Role).RoleName;
                cnt++;
            }
            if (r.DisplayName != (role as Role).DisplayName)
            {
                r.DisplayName = (role as Role).DisplayName;
                cnt++;
            }
            if (cnt > 0)
            {
                CallContext cctx = _cctx.CreateCopy();
                RoleServiceProxy rsvc = new RoleServiceProxy();
                await rsvc.AddOrUpdateEntitiesAsync(cctx, new RoleSet(), new Role[] { r });
            }
        }

        public async Task<string[]> GetUsersInRoleAsync(string rolename)
        {
            CallContext cctx = _cctx.CreateCopy();
            try
            {
                var find = await findRoleAsync(rolename);
                Role r = find != null ? find.Item1 : null;
                if (r == null)
                    return new string[] { };
                RoleServiceProxy rsvc = new RoleServiceProxy();
                var ra = await rsvc.LoadEntityHierarchyRecursAsync(cctx, r, 0, -1);
                //for a given role, the users in it also include the ones in all its child roles, recursively (see above), in addition to its own ...
                List<string> luns = new List<string>();
                await _getUserInRoleAsync(cctx, ra, luns);
                return luns.ToArray();
            }
            finally
            {
            }
        }

        private async Task _getUserInRoleAsync(CallContext cctx, EntityAbs<Role> ra, List<string> usersinrole)
        {
            UserServiceProxy usvc = new UserServiceProxy();
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { new QToken { TkName = "Username" } });
            qexpr.FilterTks = new List<QToken>(new QToken[]{
                    new QToken { TkName = "UsersInRole." },
                    new QToken { TkName = "RoleID" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "" + ra.DataBehind.ID + "" }
                });
            var users = await usvc.QueryDatabaseAsync(cctx, new UserSet(), qexpr);
            foreach (User u in users)
                usersinrole.Add(u.Username);
            if (ra.ChildEntities != null)
            {
                foreach (var c in ra.ChildEntities)
                    await _getUserInRoleAsync(cctx, c, usersinrole);
            }
        }

        private async Task<Tuple<Role, Role>> findRoleAsync(string rolename)
        {
            if (string.IsNullOrEmpty(rolename))
                return null;
            CallContext cctx = _cctx.CreateCopy();
            string[] rolepath = rolename.Trim('.').Split('.');
            RoleServiceProxy rsvc = new RoleServiceProxy();
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { new QToken { TkName = "RoleName" } });
            qexpr.FilterTks = new List<QToken>(new QToken[]{
                    new QToken { TkName = "ApplicationID" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "\"" + app.ID + "\"" },
                    new QToken { TkName = "&&" },
                    new QToken { TkName = "ParentID" },
                    new QToken { TkName = "is null" }
                });
            var rrts = await rsvc.QueryDatabaseAsync(cctx, new RoleSet(), qexpr);
            Role last = null;
            foreach (var rr in rrts)
            {
                if (rr.RoleName == rolepath[0])
                {
                    if (rolepath.Length > 1)
                    {
                        var rtree = await rsvc.LoadEntityFullHierarchyRecursAsync(cctx, rr);
                        last = rtree.DataBehind;
                        var r = findMatch(rtree, rolepath, 1, ref last);
                        return new Tuple<Role, Role>(r, last);
                    }
                    else
                    {
                        return new Tuple<Role, Role>(rr, rr);
                    }
                }
            }
            return null;
        }

        private Role findMatch(EntityAbs<Role> ra, string[] path, int lev, ref Role last)
        {
            if (ra.ChildEntities != null)
            {
                foreach (var c in ra.ChildEntities)
                {
                    if (c.DataBehind.RoleName == path[lev])
                    {
                        c.DataBehind.UpperRef = last;
                        last = c.DataBehind;
                        if (lev == path.Length - 1)
                            return c.DataBehind;
                        else
                            return findMatch(c, path, lev + 1, ref last);
                    }
                }
            }
            return null;
        }

        private async Task<string> rolePathAsync(Role r)
        {
            RoleServiceProxy rsvc = null;
            string rpath = r.RoleName;
            while (r.ParentID != null)
            {
                if (r.UpperRef == null)
                {
                    if (rsvc == null)
                        rsvc = new RoleServiceProxy();
                    r.UpperRef = await rsvc.MaterializeUpperRefAsync(_cctx.CreateCopy(), r);
                }
                rpath = r.UpperRef.RoleName + "." + rpath;
                r = r.UpperRef;
            }
            return rpath;
        }

        private void WriteToEventLog(Exception e, string action)
        {
            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action;
            Trace.Write(message);
            Debug.Write(message);
            /*
            if (log.IsErrorEnabled)
                log.Error("[" + cctx.InVokePath + "]: " + message, e);
            */
        }

        public void Dispose()
        {

        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
