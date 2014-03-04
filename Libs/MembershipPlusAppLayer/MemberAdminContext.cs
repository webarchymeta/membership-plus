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
    public class MemberAdminContext
    {
        internal static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext;
            }
        }

        public static async Task<RolePriority> GetMaxPriority(string uid)
        {
            UserServiceProxy usrv = new UserServiceProxy();
            var u = await usrv.LoadEntityByKeyAsync(Cntx, uid);
            if (u == null)
                return new RolePriority { Major = -1, Minor = -1 };
            return await GetMaxPriority(u);
        }

        public static async Task<RolePriority> GetMaxPriority(User u)
        {
            UserServiceProxy usrv = new UserServiceProxy();
            var l = await usrv.MaterializeAllUsersInRole_UserIDsAsync(Cntx, u);
            if (l == null || l.Count() == 0)
                return new RolePriority { Major = -1, Minor = -1 };
            UsersInRoleServiceProxy uisvc = new UsersInRoleServiceProxy();
            foreach (var ir in l)
                ir.RoleRef = uisvc.MaterializeRoleRef(Cntx, ir);
            var uir = (from d in l orderby d.RoleRef.RolePriority descending, d.SubPriority descending select d).First();
            var roleids = (from d in l orderby d.RoleRef.RolePriority select d.RoleID).ToArray();
            return new RolePriority { Major = uir.RoleRef.RolePriority, Minor = uir.SubPriority, RoleIds = roleids, MaxRole = uir.RoleRef };
        }

        public static async Task<string> GetSetInfo(string adminId, string set)
        {
            EntitySetType type;
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            if (Enum.TryParse<EntitySetType>(sobj["set"], out type))
            {
                string filter = null;
                if (sobj.ContainsKey("setFilter"))
                    filter = sobj["setFilter"];
                switch (type)
                {
                    case EntitySetType.User:
                        {
                            var p = await GetMaxPriority(adminId);
                            UserServiceProxy svc = new UserServiceProxy();
                            var si = await svc.GetSetInfoAsync(Cntx, filter);
                            RoleServiceProxy rsvc = new RoleServiceProxy();
                            var roles = await rsvc.QueryDatabaseAsync(Cntx, new RoleSet(), null);
                            List<dynamic> rlist = new List<dynamic>();
                            foreach (var r in roles)
                            {
                                if (r.RolePriority <= p.Major)
                                    rlist.Add(new { id = r.ID, name = r.RoleName, path = r.DistinctString, op = true });
                            }
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            string json = ser.Serialize(new { EntityCount = si.EntityCount, Sorters = si.Sorters, roles = rlist.ToArray() });
                            return json;
                        }
                }
            }
            return null;
        }

        public static async Task<string> GetManagedUsers(string adminId, string set, string qexpr, string prevlast)
        {
            var maxp = await GetMaxPriority(adminId);
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            EntitySetType type;
            if (Enum.TryParse<EntitySetType>(sobj["set"], out type))
            {
                switch (type)
                {
                    case EntitySetType.User:
                        {
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
                            RoleServiceProxy rsvc = new RoleServiceProxy();
                            var roles = await rsvc.QueryDatabaseAsync(Cntx, new RoleSet(), null);
                            var result = await svc.GetPageItemsAsync(Cntx, _set, _qexpr, _prevlast);
                            var ar = new List<dynamic>();
                            string appId = ApplicationContext.App.ID;
                            UsersInRoleServiceProxy uirsvc = new UsersInRoleServiceProxy();
                            foreach (var e in result)
                            {
                                List<Role> rlist = new List<Role>();
                                foreach (var r in roles)
                                {
                                    if (r.RolePriority <= maxp.Major)
                                        rlist.Add(r);
                                }
                                var p = await GetMaxPriority(e);
                                List<dynamic> rolelist = new List<dynamic>();
                                var _roles = await svc.MaterializeAllUsersInRole_UserIDsAsync(Cntx, e);
                                dynamic _max = null;
                                if (_roles != null)
                                {
                                    List<UsersInRole> _rlist = new List<UsersInRole>();
                                    foreach (var ir in _roles)
                                    {
                                        ir.RoleRef = uirsvc.MaterializeRoleRef(Cntx, ir);
                                        _rlist.Add(ir);
                                    }
                                    foreach (var ir in from d in _rlist orderby d.RoleRef.RolePriority descending, d.SubPriority descending select d)
                                    {
                                        bool op = adminId != e.ID && (ir.RoleRef.RolePriority < maxp.Major || ir.RoleRef.RolePriority == maxp.Major && ir.SubPriority <= maxp.Minor);
                                        var _r = new { id = ir.RoleRef.ID, uid = ir.UserID, name = ir.RoleRef.RoleName, path = Utils.GetHtmlRolePath(ir.RoleRef.DistinctString), level = ir.SubPriority, op = op };
                                        if (ir.RoleRef.ID == p.MaxRole.ID)
                                            _max = _r;
                                        rolelist.Add(_r);
                                        int ptr = -1;
                                        for (int i = 0; i < rlist.Count; i++)
                                        {
                                            if (rlist[i].ID == ir.RoleRef.ID)
                                            {
                                                ptr = i;
                                                break;
                                            }
                                        }
                                        if (ptr != -1)
                                            rlist.RemoveAt(ptr);
                                    }
                                }
                                List<dynamic> availablers = new List<dynamic>();
                                //if (adminId != e.ID)
                                {
                                    foreach (var r in rlist)
                                        availablers.Add(new { id = r.ID, name = r.RoleName, path = Utils.GetHtmlRolePath(r.DistinctString), op = true });
                                }
                                var membs = svc.MaterializeAllUserAppMembers(Cntx, e);
                                ar.Add(new { data = e, member = (from d in membs where d.ApplicationID == appId select d).SingleOrDefault(), roles = rolelist.ToArray(), maxrole = _max, availableRoles = availablers.ToArray(), CanEdit = p.IsLowerOrEqual(maxp) });
                            }
                            string json = ser3.Serialize(ar);
                            return json;
                        }
                }
            }
            return null;
        }

        public static async Task<dynamic> ResetUserPassword(string adminId, string id)
        {
            CallContext cctx = Cntx;
            try
            {
                UserServiceProxy usvc = new UserServiceProxy();
                var u = await usvc.LoadEntityByKeyAsync(cctx, id);
                if (u == null)
                    return "";
                var admin = await usvc.LoadEntityByKeyAsync(cctx, adminId);
                var maxadmp = await GetMaxPriority(adminId);
                var maxup = await GetMaxPriority(id);
                if (maxadmp.Major < maxup.Major || maxadmp.Major == maxup.Major && maxadmp.Minor < maxup.Minor)
                    return new { ok = false, msg = string.Format(ResourceUtils.GetString("0452f93e5e52c7eae26c4fac7aa2d5d7", "Denined! Your role level: {0} is less than the requested one."), maxadmp.Major.ToString() + "/" + maxadmp.Minor), newpwd = "" };
                UserStore<ApplicationUser> store = new UserStore<ApplicationUser>();
                PasswordGenerator pgen = new PasswordGenerator();
                var pwd = pgen.Generate();
                while (!pgen.Validate(pwd))
                    pwd = pgen.Generate();
                u.Password = store.HashPassword(pwd);
                if (u.IsPasswordModified)
                {
                    u.LastPasswordChangedDate = DateTime.UtcNow;
                    await usvc.AddOrUpdateEntitiesAsync(cctx, new UserSet(), new User[] { u });
                }
                return new { ok = true, msg = "", newpwd = pwd };
            }
            catch (Exception e)
            {
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("49dfe380301a10e682f1b3bc09136542", "Exception: {0}"), e.Message) };
            }
        }

        public static async Task<dynamic> ChangeMemberStatus(string adminId, string uid, string status)
        {
            UserAppMemberSet s = new UserAppMemberSet();
            if (!(from d in s.MemberStatusValues where d == status select d).Any())
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("0b8472f8e1a556b4c90b516e2df1917b", "Status '{0}' is not known."), status) };
            CallContext cctx = Cntx;
            try
            {
                UserServiceProxy usvc = new UserServiceProxy();
                UserSet us = new UserSet();
                var admin = await usvc.LoadEntityByKeyAsync(cctx, adminId);
                if (admin.ID == uid)
                    return new { ok = false, msg = ResourceUtils.GetString("0bdf4ebe91cd037e986f8260069292be", "You shouldn't lock yourself out.") };
                User u = await usvc.LoadEntityByKeyAsync(cctx, uid);
                if (u.Status != us.StatusValues[0])
                    return new { ok = false, msg = ResourceUtils.GetString("b13fb15f7b82c3438ee9e09ae6a5ba2a", "The user is locked globally. It can not be changed in a particular application.") };
                var maxadmp = await GetMaxPriority(adminId);
                var maxup = await GetMaxPriority(uid);
                if (maxadmp.Major < maxup.Major || maxadmp.Major == maxup.Major && maxadmp.Minor < maxup.Minor)
                    return new { ok = false, msg = string.Format(ResourceUtils.GetString("0452f93e5e52c7eae26c4fac7aa2d5d7", "Denined! Your role level: {0} is less than the requested one."), maxadmp.Major.ToString() + "/" + maxadmp.Minor), newpwd = "" };
                UserAppMemberServiceProxy umsrv = new UserAppMemberServiceProxy();
                UserAppMember um = await umsrv.LoadEntityByKeyAsync(cctx, ApplicationContext.App.ID, uid);
                if (um == null)
                    return new { ok = false, msg = ResourceUtils.GetString("65318cf0e6b4b76ee9ec91f92405cbb8", "Member not found!") };
                um.MemberStatus = status;
                um.LastStatusChange = DateTime.UtcNow;
                await umsrv.AddOrUpdateEntitiesAsync(cctx, s, new UserAppMember[] { um });
                return new { ok = true, msg = "" };
            }
            catch (Exception e)
            {
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("49dfe380301a10e682f1b3bc09136542", "Exception: {0}"), e.Message) };
            }
        }

        public static string GetManagedRoles(string adminId, string set, string qexpr, string prevlast)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            EntitySetType type;
            if (Enum.TryParse<EntitySetType>(sobj["set"], out type))
            {
                switch (type)
                {
                    case EntitySetType.Role:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(Role));
                            var ser3 = new JavaScriptSerializer();
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
                            RoleServiceProxy svc = new RoleServiceProxy();
                            RoleSet _set = new RoleSet();
                            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
                            _set.PageSize_ = int.Parse(sobj["pageSize"]);
                            if (sobj.ContainsKey("setFilter"))
                                _set.SetFilter = sobj["setFilter"];
                            Role _prevlast = null;
                            if (!string.IsNullOrEmpty(prevlast))
                            {
                                strm = new System.IO.MemoryStream();
                                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                                strm.Write(sbf, 0, sbf.Length);
                                strm.Position = 0;
                                _prevlast = ser2.ReadObject(strm) as Role;
                            }
                            var result = svc.GetPageItems(Cntx, _set, _qexpr, _prevlast);
                            var ar = new List<dynamic>();
                            string appId = ApplicationContext.App.ID;
                            foreach (var e in result)
                            {
                                ar.Add(new { data = e });
                            }
                            string json = ser3.Serialize(ar);
                            return json;
                        }
                }
            }
            return null;
        }
    }
}
