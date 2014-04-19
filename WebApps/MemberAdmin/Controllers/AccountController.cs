using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
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
    public class AccountController : BaseController
    {
        public AccountController()
            : this(new UserManagerEx<ApplicationUser>(new UserStore<ApplicationUser>(Startup.ClientContext, Startup.App), Startup.ClientContext, Startup.App))
        {
            (UserManager as UserManagerEx<ApplicationUser>).ExternalErrorsHandler = err => ModelState.AddModelError(err.FailType.ToString(), err.FailMessage);
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        #region Login/Logoff

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (CheckMemberInitStatus() && ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", ResourceUtils.GetString("3a2a06b3a1f05cde765219211bf2e9be", "Invalid username or password."));
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            string langs = Request.Headers["Accept-Language"];
            var callbacks = await ConnectionContext.UserConnectionClosed(User.Identity.GetUserId(), langs);
            // handle group chat ...
            {
                var gchub = new SignalRHubs.GroupChatHub();
                var nhub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<SignalRHubs.GroupChatHub>();
                if (nhub != null)
                {
                    foreach (var c in from d in callbacks where d.HubID == gchub.HubIdentity select d)
                    {
                        var peers = await GroupChatContext.ListConnectIds(gchub.HubIdentity, c.ChannelID);
                        if (peers.Length > 0)
                        {
                            var cids = (from d in peers select d.ConnectionID).ToArray();
                            nhub.Clients.Clients(cids).userDisConnected(GroupChatContext.GetJsonPeer(c));
                        }
                    }
                }
            }
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (CheckMemberInitStatus() && ModelState.IsValid)
            {
                var user = new ApplicationUser() { Username = model.UserName };
                user.Email = model.Email;
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        #endregion

        #region Personal account related

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ChangeAccountInfo(string returnUrl, ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? ResourceUtils.GetString("9bc75a1c6d94e70e8b96d8d59115c0c0", "Your password has been changed.")
                : message == ManageMessageId.SetPasswordSuccess ? ResourceUtils.GetString("9ad4e391b8ba2faf5177dcfa6dcee143", "Your password has been set.")
                : message == ManageMessageId.RemoveLoginSuccess ? ResourceUtils.GetString("9d813b903dbe8155105d3b5c4e6a04ed", "The external login was removed.")
                : message == ManageMessageId.Error ? ResourceUtils.GetString("c69732cc923305ac0684ac8fc05a4bcb", "An error has occurred.")
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = string.IsNullOrEmpty(returnUrl) ? Url.Action("ChangeAccountInfo") : returnUrl;
            ChangeAccountInfoModel model = new ChangeAccountInfoModel();
            UserServiceProxy usvc = new UserServiceProxy();
            var cntx = Startup.ClientContext.CreateCopy();
            cntx.DirectDataAccess = true;
            var u = await usvc.LoadEntityByKeyAsync(cntx, User.Identity.GetUserId());
            model.FirstName = u.FirstName;
            model.LastName = u.LastName;
            var ci = User.Identity as System.Security.Claims.ClaimsIdentity;
            model.Email = (from d in ci.Claims where d.Type == Microsoft.IdentityModel.Claims.ClaimTypes.Email select d.Value).SingleOrDefault();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> ChangeAccountInfo(ChangeAccountInfoModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { FirstName = model.FirstName, LastName = model.LastName };
                if (!string.IsNullOrEmpty(model.Email))
                    user.Email = model.Email;
                await MembershipContext.ChangeAccountInfo(User.Identity.GetUserId(), user);
            }
            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Home");
            else
                return Redirect(returnUrl);
        }

        [HttpGet]
        public ActionResult GeneratePassword()
        {
            PasswordGenerator pgen = new PasswordGenerator();
            return Json(pgen.Generate());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> ChangePassword(ChangeAccountInfoModel model, string returnUrl)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = string.IsNullOrEmpty(returnUrl) ? Url.Action("ChangeAccountInfo") : returnUrl;
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ChangeAccountInfo", new { returnUrl = ViewBag.ReturnUrl, Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ChangeAccountInfo", new { returnUrl = ViewBag.ReturnUrl, Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult GenerateUserIcon()
        {
            var m = new Models.UserIconModel();
            m.Greetings = User.Identity.GetUserName();
            m.UserLabel = m.Greetings;
            var ci = User.Identity as System.Security.Claims.ClaimsIdentity;
            string strIcon = (from d in ci.Claims where d.Type == CustomClaims.HasIcon select d.Value).SingleOrDefault();
            bool hasIcon;
            if (!string.IsNullOrEmpty(strIcon) && bool.TryParse(strIcon, out hasIcon) && hasIcon)
                m.IconUrl = "Account/GetMemberIcon?id=" + User.Identity.GetUserId();
#if NO_SIGNALR
            m.Notifications = false;
#else
            string enable = ConfigurationManager.AppSettings["EnableSignalR"];
            m.Notifications = bool.Parse(enable);
#endif
            return PartialView("_UserIconPartial", m);
        }

        [HttpGet]
        public async Task<ActionResult> GetMemberIcon(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                if (!Request.IsAuthenticated)
                    return new HttpStatusCodeResult(404, "Not Found");
                id = User.Identity.GetUserId();
            }
            var rec = await MembershipContext.GetMemberIcon(id);
            if (rec == null || string.IsNullOrEmpty(rec.MimeType))
                return new HttpStatusCodeResult(404, "Not Found");
            int status;
            string statusstr;
            bool bcache = CheckClientCache(rec.LastModified, rec.ETag, out status, out statusstr);
            SetClientCacheHeader(rec.LastModified, rec.ETag, HttpCacheability.Public);
            if (!bcache)
                return File(rec.Data, rec.MimeType);
            else
            {
                Response.StatusCode = status;
                Response.StatusDescription = statusstr;
                return Content("");
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult UpdateMemberIcon(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> UpdateMemberIconAsync(string returnUrl)
        {
            if (Request.Files != null && Request.Files.Count > 0)
            {
                if (!Request.Files[0].ContentType.StartsWith("image"))
                    throw new Exception("content mismatch!");
                string IconMime = Request.Files[0].ContentType;
                System.Nullable<DateTime> IconLastModified = default(System.Nullable<DateTime>);
                if (Request.Form.AllKeys.Contains("IconLastModified"))
                    IconLastModified = DateTime.Parse(Request.Form["IconLastModified"]);
                System.IO.Stream strm = Request.Files[0].InputStream;
                int size = Request.Files[0].ContentLength;
                byte[] data = new byte[size];
                strm.Read(data, 0, size);
                if (await MembershipContext.UpdateMemeberIcon(User.Identity.GetUserId(), IconMime, IconLastModified.Value, data))
                {
                    if (string.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Index", "Home");
                    else
                        return Redirect(returnUrl);
                }
            }
            return View();
        }

        #endregion

        #region User Details

        [HttpGet]
        public async Task<ActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                id = User.Identity.GetUserId();
            var m = await MembershipContext.GetUserDetails(id);
            return View(m);
        }

        [HttpGet]
        public async Task<ActionResult> UserPhoto(string id)
        {
            if (string.IsNullOrEmpty(id))
                id = User.Identity.GetUserId();
            var rec = await MembershipContext.GetUserPhoto(id);
            if (rec == null || string.IsNullOrEmpty(rec.MimeType))
                return new HttpStatusCodeResult(404, "Not found");
            int status;
            string statusstr;
            bool bcache = CheckClientCache(rec.LastModified, rec.ETag, out status, out statusstr);
            SetClientCacheHeader(rec.LastModified, rec.ETag, HttpCacheability.Public);
            if (bcache)
            {
                Response.StatusCode = status;
                Response.StatusDescription = statusstr;
                return Content("");
            }
            else
            {
                return File(rec.Data, rec.MimeType);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> CreateUserDetails()
        {
            await MembershipContext.CreateUserDetails(User.Identity.GetUserId());
            return RedirectToAction("UserDetails", "Account");
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> UpdateUserProperties(UserDetailsVM m)
        {
            m = await MembershipContext.UpdateUserProperties(User.Identity.GetUserId(), m);
            return View("UserDetails", m);
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> UpdateUserDescription(UserDetailsVM m)
        {
            // custom validator here ...

            m = await MembershipContext.UpdateUserDescription(User.Identity.GetUserId(), m);
            return View("UserDetails", m);
        }

        [Authorize]
        public ActionResult UpdateUserPhoto(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> UpdateUserPhotoAsync(string returnUrl)
        {
            if (Request.Files != null && Request.Files.Count > 0)
            {
                if (!Request.Files[0].ContentType.StartsWith("image"))
                    throw new Exception("content mismatch!");
                string IconMime = Request.Files[0].ContentType;
                System.Nullable<DateTime> LastModified = default(System.Nullable<DateTime>);
                if (Request.Form.AllKeys.Contains("LastModified"))
                    LastModified = DateTime.Parse(Request.Form["LastModified"]);
                System.IO.Stream strm = Request.Files[0].InputStream;
                int size = Request.Files[0].ContentLength;
                byte[] data = new byte[size];
                strm.Read(data, 0, size);
                if (await MembershipContext.UpdateUserPhoto(User.Identity.GetUserId(), IconMime, LastModified.Value, data))
                {
                    if (string.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Index", "Home");
                    else
                        return Redirect(returnUrl);
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        #endregion

        #region User Comm Channels

        [HttpPost]
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> AddChannel(int typeId, string address, string comment)
        {
            var data = await MembershipContext.AddChannel(User.Identity.GetUserId(), typeId, address, comment);
            return Json(data);
        }

        [HttpPost]
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> UpdateChannel(string id, string address, string comment)
        {
            var data = await MembershipContext.UpdateChannel(id, address, comment);
            return Json(data);
        }

        [HttpPost]
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> DeleteChannel(string id)
        {
            var data = await MembershipContext.DeleteChannel(id);
            return Json(data);
        }

        #endregion

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.Password != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion
    }
}