using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Archymeta.Web.MembershipPlus.AppLayer;
using MemberAdminMvc5.Models;
using MemberAdminMvc5.SignalRHubs;

namespace MemberAdminMvc5.Controllers
{
    public class ContactsController : Controller
    {
        private async Task PopulateTypes(ContactsQueryOpts m)
        {
            List<ContactType> l = new List<ContactType>();
            var types = await ContactContext.ListTypes();
            foreach (var type in types)
            {
                var t = new ContactType
                {
                    Id = type.ID,
                    Name = type.TypeName // map it to a more user friendly name later.
                };
                l.Add(t);
            }
            m.Types = l.ToArray();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Search(int? typeId, bool? outgoing)
        {
            ViewBag.AppName = Startup.App.Name;
            var m = new ContactsQueryOpts
            {
                UserId = User.Identity.GetUserId(),
                TypeId = typeId == null ? -1 : typeId.Value,
                Outgoing = outgoing == null ? true : outgoing.Value
            };
            await PopulateTypes(m);
            return View(m);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> GetMembers(string set, string qexpr, string prevlast, bool? outgoing)
        {
            var nhub = new NotificationHub();
            string data = await ContactContext.GetMembers(nhub.HubIdentity, User.Identity.GetUserId(), set, qexpr, prevlast, outgoing == null ? true : outgoing.Value);
            return Json(data);
        }
    }
}