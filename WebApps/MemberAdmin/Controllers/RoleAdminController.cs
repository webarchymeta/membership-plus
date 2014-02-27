using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Security.Principal;
using Microsoft.IdentityModel;
//using Microsoft.IdentityModel.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.Security;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.MembershipPlus.AppLayer;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using MemberAdminMvc5.Models;

namespace MemberAdminMvc5.Controllers
{
    [Authorize]
    public class RoleAdminController : BaseController
    {
        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> RoleAdmin()
        {
            ViewBag.AppName = Startup.App.Name;
            var rrs = await MemberRoleContext.LoadRoleSetRoots(User.Identity.GetUserId());
            ViewBag.AdminMaxPriority = rrs.adminMaxLevel;
            string roleids = "[";
            foreach (var rid in rrs.adminRoleIds)
                roleids += (roleids == "[" ? " " : ", ") + rid;
            roleids += " ]";
            ViewBag.AdminRoleIds = roleids;
            return View(rrs.roots);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> AddUserToRole(string uid, int rid)
        {
            return Json(await MemberRoleContext.AddUserToRole(User.Identity.GetUserId(), uid, rid));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> AdjustUserRoleLevel(string adminId, string uid, int rid, int del)
        {
            return Json(await MemberRoleContext.AdjustUserRoleLevel(User.Identity.GetUserId(), uid, rid, del));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> RemoveUserFromRole(string adminId, string uid, int rid)
        {
            return Json(await MemberRoleContext.RemoveUserFromRole(User.Identity.GetUserId(), uid, rid));
        }

        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> LoadRoleSetRoots()
        {
            var result = await MemberRoleContext.LoadRoleSetRoots(User.Identity.GetUserId());
            ViewBag.AdminMaxPriority = result.adminMaxLevel;
            string roleids = "[";
            foreach (var rid in result.adminRoleIds)
                roleids += (roleids == "[" ? " " : ", ") + rid;
            roleids += " ]";
            ViewBag.AdminRoleIds = roleids;
            return Json(result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> LoadRoleChildren(int pid)
        {
            return Json(await MemberRoleContext.LoadRoleChildren(User.Identity.GetUserId(), pid));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> CreateNewRole(string name, int priority, int? pid)
        {
            return Json(await MemberRoleContext.CreateNewRole(User.Identity.GetUserId(), name, priority, pid));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> UpdateRole(int id, int priority, int? pid)
        {
            return Json(await MemberRoleContext.UpdateRole(User.Identity.GetUserId(), id, priority, pid));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> DeleteRole(int id)
        {
            return Json(await MemberRoleContext.DeleteRole(User.Identity.GetUserId(), id));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> ListUsersInRole(int id)
        {
            return Json(await MemberRoleContext.ListUsersInRole(User.Identity.GetUserId(), id));
        }
    }
}