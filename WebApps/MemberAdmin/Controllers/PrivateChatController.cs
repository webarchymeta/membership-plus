using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer;
using MemberAdminMvc5.SignalRHubs;

namespace MemberAdminMvc5.Controllers
{
    public class PrivateChatController : BaseController
    {
        protected string HubId
        {
            get
            {
                if (_hubId == null)
                {
                    var hub = new SignalRHubs.PrivateChatHub();
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
        public ActionResult Search()
        {
            ViewBag.AppName = Startup.App.Name;
            ViewBag.HubId = (new NotificationHub()).HubIdentity;
            DateTime dt = DateTime.UtcNow.AddMinutes(-ApplicationContext.OnlineUserInactiveTime);
            ViewBag.TimeThreshold = (new MembershipPlusServiceProxy()).FormatRepoDateTime(dt);
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ChatPage(string toId)
        {
            if (string.IsNullOrEmpty(toId))
                return new HttpStatusCodeResult(404, "Not Found");
            else
            {
                string approot = VirtualPathUtility.ToAbsolute("~/");
                ViewBag.AppName = Startup.App.Name;
                ViewBag.UserID = User.Identity.GetUserId();
                ViewBag.User = await PrivateChatContext.LoadUserInfo(ViewBag.UserID, approot) + ";";
                ViewBag.PeerID = toId;
                ViewBag.Peer = await PrivateChatContext.LoadUserInfo(toId, approot) + ";";
                ViewBag.ListStyle = "message-list";
                ViewBag.ReplyListStyle = "reply-message-list";
                return View();
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ChatPopup(string toId)
        {
            if (string.IsNullOrEmpty(toId))
                return new HttpStatusCodeResult(404, "Not Found");
            else
            {
                string approot = VirtualPathUtility.ToAbsolute("~/");
                ViewBag.AppName = Startup.App.Name;
                ViewBag.UserID = User.Identity.GetUserId();
                ViewBag.User = await PrivateChatContext.LoadUserInfo(ViewBag.UserID, approot) + ";";
                ViewBag.PeerID = toId;
                ViewBag.Peer = await PrivateChatContext.LoadUserInfo(toId, approot) + ";";
                ViewBag.ListStyle = "popup-message-list";
                ViewBag.ReplyListStyle = "popup-reply-message-list";
                return View();
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> LoadMessages(string toId, bool? seq)
        {
            var msgs = await PrivateChatContext.LoadMessages(toId, User.Identity.GetUserId(), MaxInitMsgs, !seq.HasValue || !seq.Value);
            return Json(msgs, JsonRequestBehavior.AllowGet);
        }
    }
}