using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using log4net;
using Archymeta.Web.Logging;
using MemberAdminMvc5.Models;

namespace MemberAdminMvc5
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static ILog log = LogManager.GetLogger("Application");

        protected void Application_Start()
        {
            log4net.GlobalContext.Properties["user"] = new HttpContextUserNameProvider();
            log4net.GlobalContext.Properties["pageUrl"] = new HttpRequestUrlProvider();
            log4net.GlobalContext.Properties["referUrl"] = new HttpReferringUrlProvider();
            log4net.GlobalContext.Properties["requestId"] = new HttpRequestTraceIDProvider();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public void AnonymousIdentification_Creating(Object sender, AnonymousIdentificationEventArgs e)
        {
            e.AnonymousID = Guid.NewGuid().ToString();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            HttpException hex = Server.GetLastError() as HttpException;
            if (hex.InnerException != null)
                log.Error("Unhandled exception thrown", hex.InnerException);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            log.Info("The web application stopped");
        }
    }
}
