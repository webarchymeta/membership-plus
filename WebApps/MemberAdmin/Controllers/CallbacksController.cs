using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace MemberAdminMvc5.Controllers
{
    public class CallbacksController : BaseController
    {
        public ActionResult SystemNotification()
        {
            return View();
        }
	}
}