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
#if !NO_SIGNALR
        private Microsoft.AspNet.SignalR.IHubContext NoticeContext
        {
            get
            {
                return Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<SignalRHubs.NotificationHub>();
            }
        }
#endif

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
            var OpResult = await MemberRoleContext.AddUserToRole(User.Identity.GetUserId(), uid, rid);
            await HandleNotification(uid, OpResult);
            return Json(OpResult.Result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> AdjustUserRoleLevel(string uid, int rid, int del)
        {
            var OpResult = await MemberRoleContext.AdjustUserRoleLevel(User.Identity.GetUserId(), uid, rid, del);
            await HandleNotification(uid, OpResult);
            return Json(OpResult.Result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> RemoveUserFromRole(string uid, int rid)
        {
            var OpResult = await MemberRoleContext.RemoveUserFromRole(User.Identity.GetUserId(), uid, rid);
            await HandleNotification(uid, OpResult);
            return Json(OpResult.Result);
        }

        private async Task HandleNotification(string uid, OperationResult OpResult)
        {
            if (OpResult.notices != null)
            {
                var member = await NotificationContext.SetNotification(uid, OpResult.notices);
#if !NO_SIGNALR
                if (IsSignalREnabled && !string.IsNullOrEmpty(member.ConnectionID))
                {
                    var lcateg = await NotificationContext.GetRecentCategorized(uid, OpResult.notices, 15);
                    List<dynamic> lmsg = new List<dynamic>();
                    foreach (var n in OpResult.notices)
                    {
                        lmsg.Add(new
                        {
                            title = n.Title,
                            msg = n.Message,
                            priority = n.Priority
                        });
                    }
                    List<dynamic> categs = new List<dynamic>();
                    foreach (var categ in lcateg)
                    {
                        string categ_name = NotificationContext.MapCategoryName(categ, member.AcceptLanguages);
                        List<dynamic> l = new List<dynamic>();
                        foreach (var n in categ.ChangedMemberNotifications)
                        {
                            l.Add(new
                            {
                                title = n.DistinctString,
                                msg = "",
                                priority = n.PriorityLevel
                            });
                        }
                        categs.Add(new
                        {
                            name = categ_name,
                            list = l.ToArray()
                        });
                    }
                    NoticeContext.Clients.Client(member.ConnectionID).serverNotifications(lmsg.ToArray(), categs);
                }
#endif
            }
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