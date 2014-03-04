using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Threading;
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
    [Authorize]
    public class QueryController : BaseController
    {
        [HttpGet]
        public ActionResult SearchMembers()
        {
            ViewBag.AppName = Startup.App.Name;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetMembers(string set, string qexpr, string prevlast)
        {
            string data = await MemberViewContext.GetMembers(set, qexpr, prevlast);
            return Json(data);
        }

        [HttpGet]
        public ActionResult MemberDetails()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> MemberDetailsJson(string id)
        {
            var data = await MemberViewContext.GetBriefMemberDetails(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}