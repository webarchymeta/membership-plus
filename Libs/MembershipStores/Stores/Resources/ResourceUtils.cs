#if MemberPlus
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Threading;
using System.Globalization;
using CryptoGateway.Resources.Shared;
using CryptoGateway.Resources.Storage;

namespace Archymeta.Web.Security.Resources
{
    public enum StoreTypes
    {
        CommonShortResources,
        CommonBlockResources,
        ShortResources,
        BlockResources
    }

    public class ResourceUtils
    {
        public static CultureInfo[] GetAcceptLanguages()
        {
            HttpRequest req = HttpContext.Current.Request;
            string strlan = req.Headers["Accept-Language"];
            if (string.IsNullOrEmpty(strlan))
                return null;
            string[] lans = strlan.Split(',');
            List<CultureInfo> lc = new List<CultureInfo>();
            foreach (string lan in lans)
            {
                CultureInfo ci = GetCultureInfo(lan);
                if (ci != null)
                    lc.Add(ci);
            }
            return lc.ToArray();
        }

        private static CultureInfo GetCultureInfo(string lan)
        {
            string cn = lan.IndexOf(';') == -1 ? lan.Trim() : lan.Substring(0, lan.IndexOf(';')).Trim();
            CultureInfo ci = null;
            if (cn == "zh" || cn == "zh-chs" || cn == "zh-hans" || cn == "zh-cn")
                ci = new CultureInfo("zh-Hans");
            else if (cn == "zh-cht" || cn == "zh-hant" || cn == "zh-tw" || cn == "zh-hk" || cn == "zh-sg" || cn == "zh-mo")
                ci = new CultureInfo("zh-Hant");
            else if (cn.StartsWith("zh-"))
                ci = new CultureInfo("zh-CHS");
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

        public static CultureInfo GetEffective()
        {
            CultureInfo oci = null;
            CultureInfo[] culs = GetAcceptLanguages();
            if (culs != null)
            {
                foreach (CultureInfo ci in culs)
                {
                    string cn = ci.Name.ToLower();
                    CultureInfo _ci = null;
                    if (cn == "zh" || cn == "zh-chs" || cn == "zh-hans" || cn == "zh-cn")
                    {
                        _ci = new CultureInfo("zh-CHS");
                    }
                    else if (cn == "zh-cht" || cn == "zh-hant" || cn == "zh-tw" || cn == "zh-hk" || cn == "zh-sg" || cn == "zh-mo")
                    {
                        _ci = new CultureInfo("zh-CHT");
                    }
                    else if (cn.StartsWith("zh-"))
                    {
                        _ci = new CultureInfo("zh-CHS");
                    }
                    else
                        _ci = ci;
                    if (_ci != null)
                    {
                        oci = _ci;
                        break;
                    }
                }
            }
            return oci == null ? new CultureInfo("en") : oci;
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

        public static string GetString(string cacheId, string defval = null)
        {
            return GetString(StoreTypes.CommonShortResources, cacheId, defval);
        }

        public static string GetString(StoreTypes type, string cacheId, string defval = null)
        {
            DateTime dt;
            IUniResStore reader = null;
            switch (type)
            {
                case StoreTypes.CommonShortResources:
                    reader = Get_URL_Reader();
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
            return GetString(reader, cacheId, out dt, defval);
        }

        public static string GetString(IUniResStore reader, string cacheId, out DateTime LastModified, string defval = null)
        {
            LastModified = DateTime.MinValue;
            if (reader == null)
                return string.IsNullOrEmpty(defval) ? "string database not found...." : defval;
            int lcid = GetEffective().LCID;
            ResStrItem itm = null;
            ResStrItem ritm = new ResStrItem();
            ritm.ID = cacheId;
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