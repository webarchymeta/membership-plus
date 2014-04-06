#if MemberPlus
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Threading;
using System.Globalization;
using System.Configuration;
using CryptoGateway.Resources.Shared;
using CryptoGateway.Resources.Storage;

namespace Archymeta.Web.Security.Resources
{
    public enum StoreTypes
    {
        CommonShortResources,
        CommonBlockResources,
        ShortResources,
        QueryResources,
        BlockResources
    }

    public class ResourceUtils
    {
        public static CultureInfo[] GetAcceptLanguages(string langs)
        {
            HttpRequest req = HttpContext.Current.Request;
            string strlan = langs == null ? req.Headers["Accept-Language"] : langs;
            if (string.IsNullOrEmpty(strlan))
                return null;
            string[] lans = strlan.Split(',');
            List<Tuple<float, CultureInfo>> lc = new List<Tuple<float, CultureInfo>>();
            foreach (string lan in lans)
            {
                float weight;
                CultureInfo ci = MapCultureInfo(lan, out weight);
                if (ci != null)
                    lc.Add(new Tuple<float, CultureInfo>(weight, ci));
            }
            return (from d in lc orderby d.Item1 descending select d.Item2).ToArray();
        }

        private static CultureInfo MapCultureInfo(string lan, out float weight)
        {
            int ipos = lan.IndexOf(';');
            string cn = ipos == -1 ? lan.Trim() : lan.Substring(0, ipos).Trim();
            if (ipos == -1)
                weight = 1.0f;
            else
            {
                if (!float.TryParse(lan.Substring(ipos + 1).Trim(), out weight))
                    weight = 0.0f;
            }
            CultureInfo ci = null;
            if (cn == "zh" || cn == "zh-chs" || cn == "zh-hans" || cn == "zh-cn")
                ci = new CultureInfo("zh-Hans");
            else if (cn == "zh-cht" || cn == "zh-hant" || cn == "zh-tw" || cn == "zh-hk" || cn == "zh-sg" || cn == "zh-mo")
                ci = new CultureInfo("zh-Hant");
            else if (cn.StartsWith("zh-"))
                ci = new CultureInfo("zh-Hans");
            else
            {
                bool fail = false;
                try
                {
                    ci = new CultureInfo(cn);
                }
                catch
                {
                    fail = true;
                }
                if (fail && cn.IndexOf('-') != -1)
                {
                    cn = cn.Substring(0, cn.IndexOf('-'));
                    try
                    {
                        ci = new CultureInfo(cn);
                    }
                    catch
                    {
                    }
                }
            }
            return ci;
        }

        private static object reader_init_lock = new object();

        private static string[] SupportedLanguages
        {
            get
            {
                if (_supportedLanguages == null)
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SupportedLanguages"]))
                        _supportedLanguages = (from d in ConfigurationManager.AppSettings["SupportedLanguages"].Split(",;".ToCharArray()) select d.Trim()).ToArray();
                    else
                        _supportedLanguages = new string[] { };
                }
                return _supportedLanguages;
            }
        }
        private static string[] _supportedLanguages = null;

        public static CultureInfo GetEffective(string langs)
        {
            CultureInfo[] culs = GetAcceptLanguages(langs);
            if (culs != null)
            {
                foreach (CultureInfo ci in culs)
                {
                    string cn = ci.Name.ToLower();
                    if (SupportedLanguages.Contains(cn))
                        return ci;
                }
            }
            return new CultureInfo("en");
        }

        public static UniResStoreLiteReader Get_URL_Reader(string store = "ShortResources")
        {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/" + store + ".didxl");
            UniResStoreLiteReader reader = HttpContext.Current.Cache[path] as UniResStoreLiteReader;
            if (reader == null)
            {
                lock (reader_init_lock)
                {
                    reader = HttpContext.Current.Cache[path] as UniResStoreLiteReader;
                    if (reader == null)
                    {
                        if (System.IO.File.Exists(path))
                        {
                            reader = UniResStoreLiteReader.Load(path, CultureInfo.GetCultures(CultureTypes.NeutralCultures), true);
                            CacheDependency dep = new CacheDependency(path);
                            HttpContext.Current.Cache.Add(path, reader, dep, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(20), System.Web.Caching.CacheItemPriority.Normal, ReaderRemovedCallback);
                        }
                    }
                }
            }
            return reader;
        }

        private static void ReaderRemovedCallback(string key, object val, CacheItemRemovedReason reason)
        {
            UniResStoreLiteReader reader = val as UniResStoreLiteReader;
            reader.ShutDown();
        }

        public static string GetString(string resId, string defval = null, string langs = null)
        {
            return GetString(StoreTypes.CommonShortResources, resId, defval, langs);
        }

        public static string GetString(StoreTypes type, string resId, string defval = null, string langs = null)
        {
            DateTime dt;
            IUniResStore reader = null;
            switch (type)
            {
                case StoreTypes.CommonShortResources:
                    reader = Get_URL_Reader();
                    break;
                case StoreTypes.QueryResources:
                    reader = Get_URL_Reader("QueryResources");
                    break;
                case StoreTypes.CommonBlockResources:
                    reader = Get_URL_Reader("BlockResources");
                    break;
                case StoreTypes.ShortResources:
                    reader = Get_URL_Reader("AppShortResources");
                    break;
                case StoreTypes.BlockResources:
                    reader = Get_URL_Reader("AppBlockResources");
                    break;
                default:
                    reader = Get_URL_Reader();
                    break;
            }
            return GetString(reader, resId, out dt, defval, langs);
        }

        public static string GetString(IUniResStore reader, string resId, out DateTime LastModified, string defval = null, string langs = null)
        {
            LastModified = DateTime.MinValue;
            if (reader == null)
                return string.IsNullOrEmpty(defval) ? "string database not found...." : defval;
            int lcid = GetEffective(langs).LCID;
            ResStrItem itm = null;
            ResStrItem ritm = new ResStrItem();
            ritm.ID = resId;
            do
            {
                if (lcid != -1)
                {
                    ritm.LocID = lcid;
                    itm = reader.GetText(ritm);
                }
                else
                {
                    ritm.LocID = 0;
                    itm = reader.GetText(ritm);
                    if (itm == null)
                        return defval == null ? "string not found...." : defval;
                }
                if (itm != null)
                {
                    LastModified = itm.LastModified;
                    if (itm.ContentType.Contains("html"))
                        return itm.Text.Replace("_webroot_", VirtualPathUtility.ToAbsolute("~/"));
                    else
                        return itm.Text;
                }
                lcid = GetParentLCID(lcid);
            } while (lcid != -1);
            ritm.LocID = 0;
            itm = reader.GetText(ritm);
            if (itm == null)
                return defval == null ? "string not found...." : defval;
            LastModified = itm.LastModified;
            if (itm.ContentType.Contains("html"))
                return itm.Text.Replace("_webroot_", VirtualPathUtility.ToAbsolute("~/"));
            else
                return itm.Text;
        }

        public static int GetParentLCID(int lcid)
        {
            try
            {
                CultureInfo ci = new CultureInfo(lcid);
                return ci.Parent == null || ci.Parent.LCID == ci.LCID ? -1 : ci.Parent.LCID;
            }
            catch
            {
                return -1;
            }
        }
    }
}
#endif