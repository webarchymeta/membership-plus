using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using Archymeta.Web.Security.Resources;

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

        protected int MaxClientCacheAgeInHours
        {
            get
            {
                string sval = ConfigurationManager.AppSettings["MaxClientCacheAgeInHours"];
                int ival;
                if (string.IsNullOrEmpty(sval) || !int.TryParse(sval, out ival))
                    return 1;
                else
                    return ival;
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
            cp.AppendCacheExtension("max-age=" + 3600 * MaxClientCacheAgeInHours);
            if (ReValidate)
            {
                cp.AppendCacheExtension("must-revalidate");
                cp.AppendCacheExtension("proxy-revalidate");
            }
            cp.SetCacheability(CacheKind);
            cp.SetOmitVaryStar(false);
            if (LastModified != null)
                cp.SetLastModified(LastModified.Value);
            cp.SetExpires(DateTime.UtcNow.AddHours(MaxClientCacheAgeInHours));
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
                err.Errors.Add(ResourceUtils.GetString("8dde84756da744ecb1b23ecc193e8b25", "The membership data service is not setup or is not configured correctly!"));
                ModelState.Add(new KeyValuePair<string, ModelState>(ResourceUtils.GetString("cd4f3b92a1bfec06df534f9f6e65059a", "Member Service Failed"), err));
                return false;
            }
        }

        protected virtual ActionResult ReturnJavascript(string script)
        {
            int status;
            string statusstr;
            string etag = "";
            var h = HashAlgorithm.Create("MD5");
            etag = Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(script)));
            bool bcache = CheckClientCache(null, etag, out status, out statusstr);
            SetClientCacheHeader(null, etag, HttpCacheability.Public);
            if (!bcache)
                return JavaScript(script);
            {
                Response.StatusCode = status;
                Response.StatusDescription = statusstr;
                return Content("");
            }
        }
    }
}