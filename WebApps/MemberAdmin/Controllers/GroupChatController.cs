using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Archymeta.Web.MembershipPlus.AppLayer;
using MemberAdminMvc5.Models;

namespace MemberAdminMvc5.Controllers
{
    public class GroupChatController : BaseController
    {
        protected string HubId
        {
            get
            {
                if (_hubId == null)
                {
                    var hub = new SignalRHubs.GroupChatHub();
                    _hubId = hub.HubIdentity;
                }
                return _hubId;
            }
        }
        private string _hubId = null;

        protected int MaxInitMsgs
        {
            get
            {
                if (_maxMsgs.HasValue == false)
                {
                    string str = ConfigurationManager.AppSettings["MaxInitialChatMessages"];
                    int ival;
                    if (!string.IsNullOrEmpty(str) && int.TryParse(str, out ival))
                        _maxMsgs = ival;
                    else
                        _maxMsgs = 100;
                }
                return _maxMsgs.Value;
            }
        }
        private int? _maxMsgs = null;

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Start()
        {
            var m = await GroupChatContext.ListChatRooms(User.Identity.GetUserId());
            return View(m);
        }

        [HttpGet]
        [Authorize]
        public async Task<dynamic> LoadRoomSummary(string id)
        {
            dynamic data = await GroupChatContext.LoadRoomSummary(HubId, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ChatRoom(string id, bool? seq)
        {
            var m = await GroupChatContext.LoadChatRoom(HubId, id, User.Identity.GetUserId(), MaxInitMsgs, !seq.HasValue || !seq.Value);
            if (m.RoomExists)
                return View(m);
            else
                return new HttpStatusCodeResult(404, "Not Found");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> LoadMessages(string id, bool? seq)
        {
            var msgs = await GroupChatContext.LoadMessages(id, User.Identity.GetUserId(), MaxInitMsgs, !seq.HasValue || !seq.Value);
            return Json(msgs, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Subscribe(string id)
        {
            var s = await GroupChatContext.Subscribe(User.Identity.GetUserId(), id);
            return Json(s);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> ChangeNotification(string id)
        {
            var s = await GroupChatContext.ChangeNotification(User.Identity.GetUserId(), id);
            return Json(s);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Unsubscribe(string id)
        {
            var s = await GroupChatContext.Unsubscribe(User.Identity.GetUserId(), id);
            return Json(s);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ListConnectedMembers(string id)
        {
            return Json(await GroupChatContext.ListConnectedMembers(HubId, id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrevMessages(string k, int? v)
        {
            TimeSpanValueKind kind = k != null ? (TimeSpanValueKind)Enum.Parse(typeof(TimeSpanValueKind), k) : TimeSpanValueKind.Hours;

            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Search()
        {

            return View();
        }
    }
}