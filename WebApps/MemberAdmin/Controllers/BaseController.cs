using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MemberAdminMvc5.Controllers
{
    public class BaseController : Controller
    {
        protected bool EnableClientCache
        {
            get
            {
                string sval = ConfigurationManager.AppSettings["EnableClientCache"];
                bool bval;
                if (string.IsNullOrEmpty(sval) || !bool.TryParse(sval, out bval))
                    return false;
                else
                    return bval;
            }
        }

        private bool IsTimeGreater(DateTime t1, DateTime t2)
        {
            return t1.Year > t2.Year || t1.Month > t2.Month || t1.Date > t2.Date || t1.Hour > t2.Hour || t1.Minute > t2.Minute || t1.Second > t2.Second;
        }

        protected bool CheckClientCache(DateTime? lastModified, string Etag, out int StatusCode, out string StatusDescr)
        {
            StatusCode = 200;
            StatusDescr = "OK";
            if (!EnableClientCache)
                return false;
            bool Timed = !string.IsNullOrEmpty(Request.Headers["If-Modified-Since"]);
            bool HasEtag = !string.IsNullOrEmpty(Request.Headers["If-None-Match"]);
            if (!Timed && !HasEtag || lastModified == null && Etag == null)
                return false;
            DateTime? cacheTime = null;
            if (Timed)
                cacheTime = DateTime.Parse(Request.Headers["If-Modified-Since"]).ToUniversalTime();
            string OldEtag = HasEtag ? Request.Headers["If-None-Match"] : null;
            if (Timed && HasEtag)
            {
                if (lastModified != null && !IsTimeGreater(lastModified.Value.ToUniversalTime(), cacheTime.Value) && OldEtag == Etag)
                {
                    StatusCode = 304;
                    StatusDescr = "Not Modified";
                    return true;
                }
            }
            else if (Timed)
            {
                if (lastModified != null && !IsTimeGreater(lastModified.Value.ToUniversalTime(), cacheTime.Value))
                {
                    StatusCode = 304;
                    StatusDescr = "Not Modified";
                    return true;
                }
            }
            else if (HasEtag)
            {
                if (OldEtag == Etag)
                {
                    StatusCode = 304;
                    StatusDescr = "Not Modified";
                    return true;
                }
            }
            return false;
        }

        protected void SetClientCacheHeader(DateTime? LastModified, string Etag, HttpCacheability CacheKind, bool ReValidate = true)
        {
            if (!EnableClientCache || LastModified == null && Etag == null)
                return;
            HttpCachePolicyBase cp = Response.Cache;
            cp.AppendCacheExtension("max-age=3600");
            if (ReValidate)
            {
                cp.AppendCacheExtension("must-revalidate");
                cp.AppendCacheExtension("proxy-revalidate");
            }
            cp.SetCacheability(CacheKind);
            cp.SetOmitVaryStar(false);
            if (LastModified != null)
                cp.SetLastModified(LastModified.Value);
            cp.SetExpires(DateTime.UtcNow.AddSeconds(3600));
            if (Etag != null)
                cp.SetETag(Etag);
        }

        protected bool CheckMemberInitStatus()
        {
            if (Startup.MemberInitSuccess)
                return true;
            else
            {
                ModelState err = new System.Web.Mvc.ModelState();
                err.Errors.Add("The membership data service is not setup or is not configured correctly!");
                ModelState.Add(new KeyValuePair<string, ModelState>("Member Service Failed", err));
                return false;
            }
        }
    }
}