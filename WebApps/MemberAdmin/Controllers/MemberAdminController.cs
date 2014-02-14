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
    public class MemberAdminController : BaseController
    {
        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public ActionResult UserAdmin()
        {
            ViewBag.AppName = Startup.App.Name;
            return View();
        }
    }
}