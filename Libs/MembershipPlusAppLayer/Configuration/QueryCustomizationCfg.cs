using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer.Configuration
{
    public class QueryCustomizationHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            return new QueryCustomization(section);
        }
    }

    public enum TokenMatchKind
    {
        Contains,
        StartsWith,
        EndsWith,
        Equals,
        Regex,
        Expression
    }

    public enum TokenMatchTarget
    {
        Sorting,
        Filtering,
        All
    }

    public class TokenFilter
    {
        public bool IsAllowed
        {
            get { return _isAllowed; }
            set { _isAllowed = value; }
        }
        private bool _isAllowed = false;

        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
            set { _isCaseSensitive = value; }
        }
        private bool _isCaseSensitive = false;

        public TokenMatchKind Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }
        private TokenMatchKind _kind = TokenMatchKind.Contains;

        public TokenMatchTarget Target
        {
            get { return _target; }
            set { _target = value; }
        }
        private TokenMatchTarget _target = TokenMatchTarget.All;

        public string FilterExpr
        {
            get;
            set;
        }
    }

    public class SetFilters
    {
        public bool AllowImplied
        {
            get;
            set;
        }

        public TokenFilter[] Filters
        {
            get;
            set;
        }
    }

    public class TokenMapRec
    {
        public string ToName
        {
            get;
            set;
        }

        public string ToID
        {
            get;
            set;
        }
    }

    public class GlobalRec
    {
        public Dictionary<string, TokenMapRec> tokenMaps = new Dictionary<string, TokenMapRec>();
    }

    public class DataSourceRec
    {
        public Dictionary<string, Dictionary<string, TokenMapRec>> TokenMaps = new Dictionary<string, Dictionary<string, TokenMapRec>>();
        public Dictionary<string, SetFilters> TokenAdminFilters = new Dictionary<string, SetFilters>();
        public Dictionary<string, SetFilters> TokenAppFilters = new Dictionary<string, SetFilters>();

    }

    public class QueryCustomization
    {
        public GlobalRec GlobalTable = null;
        public Dictionary<string, DataSourceRec> MapTable = new Dictionary<string, DataSourceRec>();
        public bool ConfigExists = false;
       
        private bool IsValidSetName(string src, string set)
        {
            switch(src)
            {
                case "MembershipPlus":
                    {
                        CryptoGateway.RDB.Data.MembershipPlus.EntitySetType x;
                        return Enum.TryParse<CryptoGateway.RDB.Data.MembershipPlus.EntitySetType>(set, out x);
                    }
            }
            return false;
        }

        public QueryCustomization(XmlNode node)
        {
            XmlNode xcommon = node.SelectSingleNode("global");
            ConfigExists = xcommon != null;
            if (xcommon != null)
            {
                GlobalTable = new GlobalRec();
                Dictionary<string, TokenMapRec> maps = GlobalTable.tokenMaps;
                var xmaps = xcommon.SelectNodes("maps/map");
                foreach (XmlNode xmap in xmaps)
                {
                    bool glb = xmap.Attributes["globalize"] == null || xmap.Attributes["globalize"].Value.ToLower() == "true";
                    string toid = xmap.Attributes["to-resId"] == null || !glb ? null : xmap.Attributes["to-resId"].Value;
                    string toname = xmap.Attributes["to"] == null ? "" : xmap.Attributes["to"].Value;
                    if (toid == null)
                        maps.Add(xmap.Attributes["from"].Value, new TokenMapRec { ToName = toname });
                    else
                        maps.Add(xmap.Attributes["from"].Value, new TokenMapRec { ToID = toid });
                }
            }
            XmlNodeList xdataSrcs = node.SelectNodes("datasource");
            if (xdataSrcs == null)
                return;
            ConfigExists = true;
            foreach (XmlNode xdatasrc in xdataSrcs)
            {
                XmlNodeList xsets = xdatasrc.SelectNodes("set");
                DataSourceRec rec = new DataSourceRec();
                string src =xdatasrc.Attributes["name"].Value;
                MapTable.Add(src, rec);
                foreach (XmlNode xset in xsets)
                {
                    string set = xset.Attributes["name"].Value;
                    if (!IsValidSetName(src, set))
                        continue;
                    Dictionary<string, TokenMapRec> maps = new Dictionary<string, TokenMapRec>();
                    rec.TokenMaps.Add(set, maps);
                    var xmaps = xset.SelectNodes("maps/map");
                    foreach (XmlNode xmap in xmaps)
                    {
                        bool glb = xmap.Attributes["globalize"] == null || xmap.Attributes["globalize"].Value.ToLower() == "true";
                        string toid = xmap.Attributes["to-resId"] == null || !glb ? null : xmap.Attributes["to-resId"].Value;
                        string toname = xmap.Attributes["to"] == null ? "" : xmap.Attributes["to"].Value;
                        if (toid == null)
                            maps.Add(xmap.Attributes["from"].Value, new TokenMapRec { ToName = toname });
                        else
                            maps.Add(xmap.Attributes["from"].Value, new TokenMapRec { ToID = toid });
                    }
                    //
                    var xadminfilters = xset.SelectSingleNode("filters[@type='admin']");
                    SetFilters setadminflts = new SetFilters();
                    setadminflts.AllowImplied = xadminfilters.Attributes["allow-implied"] == null || xadminfilters.Attributes["allow-implied"].Value.ToLower() == "true" ? true : false;
                    List<TokenFilter> l = new List<TokenFilter>();
                    foreach (XmlNode n in xadminfilters.SelectNodes("filter"))
                    {
                        l.Add(getFilter(n, setadminflts.AllowImplied));
                    }
                    setadminflts.Filters = l.ToArray();
                    rec.TokenAdminFilters.Add(set, setadminflts);
                    //
                    var xappfilters = xset.SelectSingleNode("filters[@type='app']");
                    SetFilters setappflts = new SetFilters();
                    setappflts.AllowImplied = xappfilters.Attributes["allow-implied"] == null || xappfilters.Attributes["allow-implied"].Value.ToLower() == "true" ? true : false;
                    l = new List<TokenFilter>();
                    foreach (XmlNode n in xappfilters.SelectNodes("filter"))
                    {
                        l.Add(getFilter(n, setappflts.AllowImplied));
                    }
                    setappflts.Filters = l.ToArray();
                    rec.TokenAppFilters.Add(set, setappflts);
                }
            }
        }

        private TokenFilter getFilter(XmlNode n, bool allow_implied)
        {
            TokenFilter filter = new TokenFilter();
            if (n.Attributes["target"] != null)
                filter.Target = n.Attributes["target"].Value == "sorting" ? TokenMatchTarget.Sorting : n.Attributes["target"].Value == "filtering" ? TokenMatchTarget.Filtering : TokenMatchTarget.All;
            filter.IsAllowed = n.Attributes["allowed"] == null ? !allow_implied : n.Attributes["allowed"].Value.ToLower() == "true" ? true : false;
            filter.IsCaseSensitive = n.Attributes["case-sensitive"] != null && n.Attributes["case-sensitive"].Value.ToLower() == "true";
            string expr = n.Attributes["expr"].Value.Trim();
            if (expr.StartsWith("*") && expr.EndsWith("*"))
            {
                filter.Kind = TokenMatchKind.Contains;
                filter.FilterExpr = expr.Trim('*');
            }
            else if (expr.EndsWith("*"))
            {
                filter.Kind = TokenMatchKind.StartsWith;
                filter.FilterExpr = expr.TrimEnd('*');
            }
            else if (expr.StartsWith("*"))
            {
                filter.Kind = TokenMatchKind.EndsWith;
                filter.FilterExpr = expr.TrimStart('*');
            }
            else if (expr.StartsWith("[") && expr.EndsWith("]"))
            {
                filter.Kind = TokenMatchKind.Regex;
                filter.FilterExpr = expr.Trim("[]".ToCharArray());
            }
            else if (Regex.IsMatch(expr, @"\{\d+\}"))
            {
                filter.Kind = TokenMatchKind.Expression;
                filter.FilterExpr = expr;
            }
            else
            {
                filter.Kind = TokenMatchKind.Equals;
                filter.FilterExpr = expr;
            }
            return filter;
        }

        public Dictionary<string, Dictionary<string, TokenMapRec>> GetMaps(string src)
        {
            DataSourceRec rec;
            if (MapTable.TryGetValue(src, out rec))
                return rec.TokenMaps;
            else
                return null;
        }

        public SetFilters GetAdminFilters(string src, string set)
        {
            DataSourceRec rec;
            if (MapTable.TryGetValue(src, out rec))
                return rec.TokenAdminFilters.ContainsKey(set) ? rec.TokenAdminFilters[set] : null;
            else
                return null;
        }

        public SetFilters GetAppFilters(string src, string set)
        {
            DataSourceRec rec;
            if (MapTable.TryGetValue(src, out rec))
                return rec.TokenAppFilters.ContainsKey(set) ? rec.TokenAppFilters[set] : null;
            else
                return null;
        }
    }
}
