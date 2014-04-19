using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MemberAdminMvc5.SignalRHubs;

namespace MemberAdminMvc5.Controllers
{
    public class PrivateChatController : BaseController
    {
        public ActionResult Search()
        {
            ViewBag.AppName = Startup.App.Name;
            ViewBag.HubId = (new NotificationHub()).HubIdentity;
            return View();
        }
                
        public ActionResult Contacts()
        {
            ViewBag.AppName = Startup.App.Name;
            ViewBag.HubId = (new NotificationHub()).HubIdentity;
            return View();
        }

        public ActionResult ChatPage(string toId)
        {
            if (string.IsNullOrEmpty(toId))
                return new HttpStatusCodeResult(404, "Not Found");
            else
            {
                ViewBag.AppName = Startup.App.Name;
                ViewBag.UserID = User.Identity.GetUserId();
                ViewBag.PeerID = toId;
                return View();
            }
        }

        public ActionResult ChatPopup(string toId)
        {
            if (string.IsNullOrEmpty(toId))
                return new HttpStatusCodeResult(404, "Not Found");
            else
            {
                ViewBag.AppName = Startup.App.Name;
                ViewBag.UserID = User.Identity.GetUserId();
                ViewBag.PeerID = toId;
                return View();
            }
        }
    }
}