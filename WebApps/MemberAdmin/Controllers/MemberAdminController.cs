using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Configuration;
using System.Security.Principal;
using Microsoft.IdentityModel;
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
    public class MemberAdminController : BaseController
    {
        private Microsoft.AspNet.SignalR.IHubContext NoticeContext
        {
            get
            {
                return Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<SignalRHubs.NotificationHub>();
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public ActionResult UserAdmin()
        {
            ViewBag.AppName = Startup.App.Name;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> GetSetInfo(string set)
        {
            string data = await MemberAdminContext.GetSetInfo(User.Identity.GetUserId(), set);
            return Json(data);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> GetManagedUsers(string set, string qexpr, string prevlast)
        {
            string data = await MemberAdminContext.GetManagedUsers(User.Identity.GetUserId(), set, qexpr, prevlast);
            return Json(data);
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> ResetUserPassword(string userId)
        {
            return Json(await MemberAdminContext.ResetUserPassword(User.Identity.GetUserId(), userId));
        }

        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<ActionResult> ChangeMemberStatus(string uid, string status)
        {
            return Json(await MemberAdminContext.ChangeMemberStatus(User.Identity.GetUserId(), uid, status));
        }

        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public ActionResult OnlineUsers()
        {
            var svc = new MembershipPlusServiceProxy();
            string filter = "UserAppMember.Application_Ref.ID == \"" + ApplicationContext.App.ID + "\" && ( UserAppMember.SearchListing is null || UserAppMember.SearchListing == true ) && ";
            filter += "UserAppMember.ConnectionID is not null && UserAppMember.LastActivityDate > ";
            ViewBag.SetFilter = filter;
            ViewBag.TimeWindow = ApplicationContext.OnlineUserInactiveTime;
            ViewBag.AppNam = ApplicationContext.App.Name;
            return View();
        }

    }
}