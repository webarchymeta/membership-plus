using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace MemberAdminMvc5.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [ChildActionOnly]
        public ActionResult GetSystemNotificationWrapper()
        {
            string enable = ConfigurationManager.AppSettings["EnableSignalR"];
            if (enable.ToLower() == "true")
                return PartialView("~/Views/Callbacks/_SysNotificationWraper.cshtml");
            else
                return Content("");
        }
    }
}