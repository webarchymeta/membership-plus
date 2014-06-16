using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace MemberAdminMvc5.Models
{
    public class HttpContextUserNameProvider
    {
        public override string ToString()
        {
            HttpContext c = HttpContext.Current;
            if (c != null)
            {
                if (c.User != null && c.User.Identity.IsAuthenticated)
                    return c.User.Identity.Name;
                else
                    return c.Request != null && c.Request.AnonymousID != null ? c.Request.AnonymousID : getRequestIP();
            }
            else
            {
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                    return Thread.CurrentPrincipal.Identity.Name;
                else
                    return getRequestIP();
            }
        }

        private string getRequestIP()
        {
            HttpContext c = HttpContext.Current;
            if (c != null)
                return c.Request.UserHostAddress;
            else
                return "Request From Unknown User";
        }
    }

    public class HttpRequestUrlProvider
    {
        public override string ToString()
        {
            HttpContext c = HttpContext.Current;
            if (c != null && c.Request != null)
                return c.Request.RawUrl;
            else
                return null;
        }
    }

    public class HttpReferringUrlProvider
    {
        public override string ToString()
        {
            HttpContext c = HttpContext.Current;
            if (c != null && c.Request != null)
                return c.Request.UrlReferrer == null ? null : c.Request.UrlReferrer.ToString();
            else
                return null;
        }
    }

    public class HttpRequestTraceIDProvider
    {
        public override string ToString()
        {
            HttpContext c = HttpContext.Current;
            if (c != null && c.Request != null)
                return c.Items["RequestTraceID"] == null ? null : c.Items["RequestTraceID"].ToString();
            else
                return null;
        }
    }
}
