using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using Archymeta.Web.Security;
using Archymeta.Web.Security.Resources;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class RoleSetRoots
    {
        public List<dynamic> roots;
        public int adminMaxLevel;
        public int[] adminRoleIds;
    }

    public class MemberRoleContext
    {
        public enum UserRoleOperations
        {
            Added,
            Modified,
            Deleted
        }

        private static CallContext Cntx
        {
            get { return ApplicationContext.ClientContext.CreateCopy(); }
        }

        public static bool DBAutoCleanupRoles
        {
            get
            {
                if (_dbAutoCleanupRoles.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["UserStoreAutoCleanupRoles"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _dbAutoCleanupRoles = bv;
                    else
                        _dbAutoCleanupRoles = false;
                }
                return _dbAutoCleanupRoles.Value;
            }
        }
        private static bool? _dbAutoCleanupRoles = default(bool?);

        private static bool ThrowOnPopulatedRole
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
        private static bool? _throwOnPopulatedRole = default(bool?);

        public static async Task<dynamic> AddUserToRole(string adminId, string uid, int rid)
        {
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            RoleServiceProxy rsvc = new RoleServiceProxy();
            UserServiceProxy usvc = new UserServiceProxy();
            var u = await usvc.LoadEntityByKeyAsync(Cntx, uid);
            if (u == null)
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("b66098049404e4de1356242e8aa6444a", "User \"{0}\" is not found."), uid) };
            var uroles = await usvc.MaterializeAllRolesAsync(Cntx, u);
            if (DBAutoCleanupRoles)
            {
                // prevent polution
                List<Role> higherroles = new List<Role>();
                foreach (var ur in uroles)
                {
                    var pr = ur;
                    if (pr.ID == rid)
                        higherroles.Add(ur);
                    while (pr.ParentID != null)
                    {
                        pr.UpperRef = await rsvc.MaterializeUpperRefAsync(Cntx, pr);
                        pr = pr.UpperRef;
                        if (pr.ID == rid)
                        {
                            higherroles.Add(ur);
                            break;
                        }
                    }
                }
                if (higherroles.Count > 0)
                {
                    string rolesstr = "";
                    foreach (var hr in higherroles)
                        rolesstr += (rolesstr == "" ? "" : ", ") + hr.DistinctString;
                    string errorfmt = ResourceUtils.GetString("43558b5deaec392b9461d28d4e753687", "Operation denied: the user already has this or more specific roles: '{0}'! Try to remove them before adding present one.");
                    return new { ok = false, msg = string.Format(errorfmt, rolesstr) };
                }
            }
            var r = await rsvc.LoadEntityByKeyAsync(Cntx, rid);
            if (r == null)
                return new { ok = false, msg = ResourceUtils.GetString("db2a3d7bc44d36a9ebeaa0d562c4cd21", "The role is not found.") };
            else if (r.RolePriority > maxp.Major)
                return new { ok = false, msg = ResourceUtils.GetString("67729f0f407d1ea57f28b43235b3e5f6", "Adding more priviledged role is not authorized.") };
            var uir = new UsersInRole();
            List<Role> removed = new List<Role>();
            if (DBAutoCleanupRoles)
            {
                // clean up: find more general roles to remove.
                var p = r;
                while (p.ParentID != null)
                {
                    p.UpperRef = await rsvc.MaterializeUpperRefAsync(Cntx, p);
                    p = p.UpperRef;
                    foreach (var ur in uroles)
                    {
                        if (ur.ID == p.ID)
                        {
                            if (!(from d in removed where d.ID == p.ID select d).Any())
                                removed.Add(p);
                        }
                    }
                }
            }
            uir.IsPersisted = false;
            uir.UserID = u.ID;
            uir.RoleID = rid;
            uir.SubPriority = 0;
            uir.AssignDate = DateTime.UtcNow;
            uir.LastModified = uir.AssignDate;
            uir.AdminID = adminId;
            UsersInRoleServiceProxy uirsvc = new UsersInRoleServiceProxy();
            await uirsvc.AddOrUpdateEntitiesAsync(Cntx, new UsersInRoleSet(), new UsersInRole[] { uir });
            var _r = new { id = rid, uid = u.ID, name = r.RoleName, path = r.DistinctString, level = uir.SubPriority, op = true };
            List<dynamic> _removed = new List<dynamic>();
            if (removed.Count > 0)
            {
                List<UsersInRole> l = new List<UsersInRole>();
                foreach (var rmv in removed)
                {
                    var x = uirsvc.LoadEntityByKey(Cntx, rmv.ID, u.ID);
                    l.Add(x);
                    _removed.Add(new { id = rmv.ID, name = rmv.RoleName, path = rmv.DistinctString, op = maxp.Major >= rmv.RolePriority });
                }
                uirsvc.DeleteEntities(Cntx, new UsersInRoleSet(), l.ToArray());
            }
            await AddUserRoleHistory(uir, UserRoleOperations.Added);
            return new { ok = true, msg = "", added = _r, removed = _removed.ToArray() };
        }

        public static async Task<dynamic> AdjustUserRoleLevel(string adminId, string uid, int rid, int del)
        {
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            UserServiceProxy usvc = new UserServiceProxy();
            var u = usvc.LoadEntityByKey(Cntx, uid);
            if (u == null)
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("b66098049404e4de1356242e8aa6444a", "User \"{0}\" is not found."), uid) };
            UsersInRoleServiceProxy uirsvc = new UsersInRoleServiceProxy();
            var uir = await uirsvc.LoadEntityByKeyAsync(Cntx, rid, u.ID);
            if (uir == null)
                return new { ok = false, msg = ResourceUtils.GetString("78257cace857db766d54e6568d7f912b", "The user is not in this role.") };
            uir.RoleRef = await uirsvc.MaterializeRoleRefAsync(Cntx, uir);
            if (maxp.Major < uir.RoleRef.RolePriority || maxp.Major == uir.RoleRef.RolePriority && uir.SubPriority + del > maxp.Major)
                return new { ok = false, msg = ResourceUtils.GetString("5986d63fe301793ee7f5b2134a8f8787", "Modifying more priviledged role is not authorized.") };
            uir.SubPriority += del;
            uir.LastModified = DateTime.UtcNow;
            uir.AdminID = adminId;
            await uirsvc.AddOrUpdateEntitiesAsync(Cntx, new UsersInRoleSet(), new UsersInRole[] { uir });
            uir.UserID = u.ID;
            uir.RoleID = rid;
            await AddUserRoleHistory(uir, UserRoleOperations.Modified);
            return new { ok = true, msg = "" };
        }

        public static async Task<dynamic> RemoveUserFromRole(string adminId, string uid, int rid)
        {
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            UserServiceProxy usvc = new UserServiceProxy();
            var u = await usvc.LoadEntityByKeyAsync(Cntx, uid);
            if (u == null)
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("b66098049404e4de1356242e8aa6444a", "User \"{0}\" is not found."), uid) };
            UsersInRoleServiceProxy uirsvc = new UsersInRoleServiceProxy();
            var uir = await uirsvc.LoadEntityByKeyAsync(Cntx, rid, u.ID);
            if (uir == null)
                return new { ok = false, msg = ResourceUtils.GetString("78257cace857db766d54e6568d7f912b", "The user is not in this role.") };
            uir.RoleRef = await uirsvc.MaterializeRoleRefAsync(Cntx, uir);
            if (maxp.Major < uir.RoleRef.RolePriority || maxp.Major == uir.RoleRef.RolePriority && uir.SubPriority > maxp.Major)
                return new { ok = false, msg = ResourceUtils.GetString("0437b5660f17723dc29c3fa7e08e08a0", "Removing more priviledged role is not authorized.") };
            await uirsvc.DeleteEntitiesAsync(Cntx, new UsersInRoleSet(), new UsersInRole[] { uir });
            uir.UserID = u.ID;
            uir.RoleID = rid;
            await AddUserRoleHistory(uir, UserRoleOperations.Deleted);
            return new { ok = true, msg = "", available = new { id = rid, name = uir.RoleRef.RoleName, path = uir.RoleRef.DistinctString, op = true } };
        }

        private static async Task AddUserRoleHistory(UsersInRole current, UserRoleOperations op)
        {
            UsersRoleHistoryServiceProxy hrhsvc = new UsersRoleHistoryServiceProxy();
            UsersRoleHistorySet urhs = new UsersRoleHistorySet();
            UsersRoleHistory urh = new UsersRoleHistory();
            urh.UserID = current.UserID;
            urh.RoleID = current.RoleID;
            urh.SubPriority = current.SubPriority;
            urh.OperatorID = current.AdminID;
            urh.ChangeDate = DateTime.UtcNow;
            switch (op)
            {
                case UserRoleOperations.Added:
                    urh.Operation = urhs.OperationValues[0];
                    break;
                case UserRoleOperations.Modified:
                    urh.Operation = urhs.OperationValues[1];
                    break;
                case UserRoleOperations.Deleted:
                    urh.Operation = urhs.OperationValues[2];
                    break;
            }
            await hrhsvc.AddOrUpdateEntitiesAsync(Cntx, urhs, new UsersRoleHistory[] { urh });
        }

        internal static dynamic MakeJsonRole(EntityAbs<Role> rabs, RolePriority admMax)
        {
            return new
            {
                id = rabs.DataBehind.ID,
                name = rabs.DataBehind.RoleName,
                path = Utils.GetHtmlRolePath(rabs.DataBehind.DistinctString),
                priority = rabs.DataBehind.RolePriority,
                hasParents = rabs.ParentExists,
                hasChilds = rabs.ChildExists,
                childsLoaded = rabs.IsChildsLoaded,
                pid = rabs.DataBehind.ParentID == null ? new Nullable<int>() : rabs.DataBehind.ParentID,
                op = admMax.Major >= rabs.DataBehind.RolePriority
            };
        }

        public static async Task<RoleSetRoots> LoadRoleSetRoots(string adminId)
        {
            RoleSetRoots rrs = new RoleSetRoots();
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            rrs.adminMaxLevel = maxp.Major;
            rrs.adminRoleIds = maxp.RoleIds;
            RoleServiceProxy svc = new RoleServiceProxy();
            var roots = svc.LoadEntitySetRoots(Cntx);
            rrs.roots = new List<dynamic>();
            foreach (var rr in roots)
                rrs.roots.Add(MakeJsonRole(rr, maxp));
            return rrs;
        }

        public static async Task<List<dynamic>> LoadRoleChildren(string adminId, int pid)
        {
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            RoleServiceProxy svc = new RoleServiceProxy();
            var p = await svc.LoadEntityByKeyAsync(Cntx, pid);
            var childs = await svc.LoadEntityChildrenAsync(Cntx, new EntityAbs<Role>(p));
            List<dynamic> jsrts = new List<dynamic>();
            foreach (var cc in childs)
                jsrts.Add(MakeJsonRole(cc, maxp));
            return jsrts;
        }

        public static async Task<dynamic> CreateNewRole(string adminId, string name, int priority, int? pid)
        {
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            if (maxp.Major < priority)
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("0452f93e5e52c7eae26c4fac7aa2d5d7", "Denined! Your role priority: {0} is less than the requested one."), maxp.Major), role = new { } };
            RoleServiceProxy rsvc = new RoleServiceProxy();
            if (pid != null)
            {
                var prole = await rsvc.LoadEntityByKeyAsync(Cntx, pid.Value);
                if (prole.RolePriority >= priority)
                    return new { ok = false, msg = string.Format(ResourceUtils.GetString("b1ac4f163f802b8cb0c9216a2845c96b", "Denined! The role priority: {0} is less than or equals to the one for the parent role."), priority), role = new { } };
            }
            Role r = new Role
            {
                RoleName = name,
                RolePriority = priority,
                ApplicationID = ApplicationContext.App.ID,
                DisplayName = name,
                ParentID = pid
            };
            var result = await rsvc.AddOrUpdateEntitiesAsync(Cntx, new RoleSet(), new Role[] { r });
            if ((result.ChangedEntities[0].OpStatus & (int)EntityOpStatus.Added) > 0)
            {
                var rabs = new EntityAbs<Role>(result.ChangedEntities[0].UpdatedItem);
                rabs.ParentExists = pid != null;
                return new { ok = true, msg = "", role = MakeJsonRole(rabs, maxp) };
            }
            else
            {
                if ((result.ChangedEntities[0].OpStatus & (int)EntityOpStatus.Duplication) > 0)
                    return new { ok = false, msg = ResourceUtils.GetString("a1794e1f262706c9409389bcfcff7499", "An existing role with the same name exists!"), role = new { } };
                else
                    return new { ok = false, msg = ResourceUtils.GetString("aa1d969415687af8bf4e3ba5e4e3bc14", "Add failed, try again?"), role = new { } };
            }
        }

        public static async Task<dynamic> UpdateRole(string adminId, int id, int priority, int? pid)
        {
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            if (maxp.Major < priority)
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("0452f93e5e52c7eae26c4fac7aa2d5d7", "Denined! Your role priority: {0} is less than the requested one."), maxp.Major), role = new { } };
            RoleServiceProxy rsvc = new RoleServiceProxy();
            if (pid != null)
            {
                var prole = await rsvc.LoadEntityByKeyAsync(Cntx, pid.Value);
                if (prole.RolePriority >= priority)
                    return new { ok = false, msg = string.Format(ResourceUtils.GetString("b1ac4f163f802b8cb0c9216a2845c96b", "Denined! The role priority: {0} is less than or equals to the one for the parent role."), priority), role = new { } };
            }
            var r = await rsvc.LoadEntityByKeyAsync(Cntx, id);
            if (r == null)
                return new { ok = false, msg = ResourceUtils.GetString("2dcb0c4ea3d378571beac6927e1a4a99", "The role is not found!") };
            if (r.RolePriority == priority)
                return new { ok = true, msg = "" };
            var _r = await rsvc.LoadEntityHierarchyRecursAsync(Cntx, r, 0, -1);
            if (_r.ChildEntities != null)
            {
                if (!checkRolePrior(priority, _r.ChildEntities))
                    return new { ok = false, msg = string.Format(ResourceUtils.GetString("680d7ee4f668b8eac69d8153e3e25293", "The attempted role priority: {0} is greater than or equals to one of child role priorities."), priority) };
            }
            r.RolePriority = priority;
            var result = await rsvc.AddOrUpdateEntitiesAsync(Cntx, new RoleSet(), new Role[] { r });
            if ((result.ChangedEntities[0].OpStatus & (int)EntityOpStatus.Updated) > 0)
            {
                var rabs = new EntityAbs<Role>(result.ChangedEntities[0].UpdatedItem);
                rabs.ParentExists = pid != null;
                return new { ok = true, msg = "" };
            }
            else
            {
                if ((result.ChangedEntities[0].OpStatus & (int)EntityOpStatus.Duplication) > 0)
                    return new { ok = false, msg = ResourceUtils.GetString("a1794e1f262706c9409389bcfcff7499", "An existing role with the same name exists!"), role = new { } };
                else
                    return new { ok = false, msg = ResourceUtils.GetString("a91c35ceb071295cf0c07ef4acc9424e", "Update failed, try again?") };
            }
        }

        private static bool checkRolePrior(int priority, List<EntityAbs<Role>> children)
        {
            foreach (var c in children)
            {
                if (c.DataBehind.RolePriority <= priority)
                    return false;
                if (c.ChildEntities != null)
                {
                    if (!checkRolePrior(priority, c.ChildEntities))
                        return false;
                }
            }
            return true;
        }

        public static async Task<dynamic> DeleteRole(string adminId, int id)
        {
            RoleServiceProxy rsvc = new RoleServiceProxy();
            var r = await rsvc.LoadEntityByKeyAsync(Cntx, id);
            if (r == null)
                return new { ok = false, msg = ResourceUtils.GetString("2dcb0c4ea3d378571beac6927e1a4a99", "The role is not found!") };
            if (!r.CanBeDeleted)
                return new { ok = false, msg = ResourceUtils.GetString("9c43a60a6e3a5c4addb0a6ed16d5e297", "The role is marked as non deletable at the application level!") };
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            if (maxp.Major < r.RolePriority)
                return new { ok = false, msg = string.Format(ResourceUtils.GetString("0452f93e5e52c7eae26c4fac7aa2d5d7", "Denined! Your role priority: {0} is less than the requested one."), maxp.Major), role = new { } };
            var childs = await rsvc.LoadEntityChildrenAsync(Cntx, new EntityAbs<Role>(r));
            if (childs.Count() > 0)
                return new { ok = false, msg = ResourceUtils.GetString("a28af96e82c950ba80f82bba8ad3e404", "The role has child roles, try to delete them before deleting this one.") };
            if (!ThrowOnPopulatedRole)
                await rsvc.DeleteEntitiesAsync(Cntx, new RoleSet(), new Role[] { r });
            else
            {
                var uirs = await rsvc.MaterializeAllUsersInRolesAsync(Cntx, r);
                if (uirs.Count() > 0)
                    return new { ok = false, msg = ResourceUtils.GetString("6de8c40a93b3d0c36fb4b5daa73d7db5", "Cannot delete a populated role.") };
            }
            return new { ok = true, msg = "" };
        }

        public static async Task<dynamic> ListUsersInRole(string adminId, int id)
        {
            RoleServiceProxy rsvc = new RoleServiceProxy();
            var r = await rsvc.LoadEntityByKeyAsync(Cntx, id);
            List<dynamic> users = new List<dynamic>();
            if (r == null)
                return new { ok = false, msg = ResourceUtils.GetString("2dcb0c4ea3d378571beac6927e1a4a99", "The role is not found!"), users = users };
            var maxp = await MemberAdminContext.GetMaxPriority(adminId);
            var uirs = await rsvc.MaterializeAllUsersInRolesAsync(Cntx, r);
            UsersInRoleServiceProxy uirsvc = new UsersInRoleServiceProxy();
            foreach (var uir in uirs)
            {
                uir.User_UserID = await uirsvc.MaterializeUser_UserIDAsync(Cntx, uir);
                uir.RoleRef = await uirsvc.MaterializeRoleRefAsync(Cntx, uir);
                var umax = await MemberAdminContext.GetMaxPriority(uir.User_UserID.ID);
                bool canOp = false;
                if (maxp.Major >= umax.Major)
                    canOp = maxp.Major > r.RolePriority || maxp.Major == r.RolePriority && maxp.Minor >= uir.SubPriority;
                users.Add(new { id = uir.RoleRef.ID, uid = uir.User_UserID.ID, name = uir.RoleRef.RoleName, username = uir.User_UserID.Username, path = Utils.GetHtmlRolePath(uir.RoleRef.DistinctString), level = uir.SubPriority, op = canOp });
            }
            return new { ok = true, msg = "", users = users };
        }
    }
}
