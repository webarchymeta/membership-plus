using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Archymeta.Web.MembershipPlus.AppLayer;

namespace MemberAdminMvc5.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ViewBag.UserID = User.Identity.GetUserId();
            var m = await NotificationContext.GetTypes();
            m.SelectedIndex = -1;
            return View(m);
        }

        [HttpGet]
        public async Task<ActionResult> CategorizedQuery(int id)
        {
            ViewBag.UserID = User.Identity.GetUserId();
            ViewBag.TypeID = id;
            var m = await NotificationContext.GetTypes();
            for (int i = 0; i < m.Types.Length; i++)
            {
                if (id == m.Types[i].ID)
                {
                    m.SelectedIndex = i;
                    break;
                }
            }
            return View(m);
        }

        [HttpPost]
        public async Task<ActionResult> GetNotifications(string set, string qexpr, string prevlast)
        {
            string data = await NotificationContext.GetNotifications(set, qexpr, prevlast);
            return Json(data);
        }

	}
}