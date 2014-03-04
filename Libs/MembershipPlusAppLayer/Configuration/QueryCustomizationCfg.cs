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

    public class QueryCustomization
    {
        private Dictionary<EntitySetType, Dictionary<string, TokenMapRec>> tokenMaps = new Dictionary<EntitySetType, Dictionary<string, TokenMapRec>>();
        private Dictionary<EntitySetType, SetFilters> tokenAdminFilters = new Dictionary<EntitySetType, SetFilters>();
        private Dictionary<EntitySetType, SetFilters> tokenAppFilters = new Dictionary<EntitySetType, SetFilters>();
       
        public QueryCustomization(XmlNode node)
        {
            XmlNode xcommon = node.SelectSingleNode("set[@name='']");
            if (xcommon != null)
            {
                Dictionary<string, TokenMapRec> maps = new Dictionary<string, TokenMapRec>();
                tokenMaps.Add(EntitySetType.DatabaseLevel, maps);
                var xmaps = xcommon.SelectNodes("maps/map");
                foreach (XmlNode xmap in xmaps)
                {
                    string toid = xmap.Attributes["toId"] == null ? null : xmap.Attributes["toId"].Value;
                    string toname = xmap.Attributes["to"] == null ? "" : xmap.Attributes["to"].Value;
                    if (toid == null)
                        maps.Add(xmap.Attributes["from"].Value, new TokenMapRec { ToName = toname });
                    else
                        maps.Add(xmap.Attributes["from"].Value, new TokenMapRec { ToID = toid });
                }
            }
            foreach (var _type in Enum.GetValues(typeof(EntitySetType)))
            {
                var type = (EntitySetType)_type;
                if (type == EntitySetType.Unknown || type == EntitySetType.DatabaseLevel)
                    continue;
                XmlNode xset = node.SelectSingleNode("set[@name='" + type + "']");
                if (xset != null)
                {
                    Dictionary<string, TokenMapRec> maps = new Dictionary<string, TokenMapRec>();
                    tokenMaps.Add(type, maps);
                    var xmaps = xset.SelectNodes("maps/map");
                    foreach (XmlNode xmap in xmaps)
                    {
                        bool glb = xmap.Attributes["globalize"] == null || xmap.Attributes["globalize"].Value.ToLower() == "true";
                        string toid = xmap.Attributes["toId"] == null || !glb ? null : xmap.Attributes["toId"].Value;
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
                    tokenAdminFilters.Add(type, setadminflts);
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
                    tokenAppFilters.Add(type, setappflts);
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

        public Dictionary<string,TokenMapRec> this[string key]
        {
            get
            {
                EntitySetType type;
                if (!Enum.TryParse<EntitySetType>(key, out type))
                    return null;
                return this[type];
            }
        }

        public Dictionary<string, TokenMapRec> this[EntitySetType type]
        {
            get
            {
                return tokenMaps.ContainsKey(type) ? tokenMaps[type] : null;
            }
        }

        public SetFilters GetAdminFilters(EntitySetType type)
        {
            return tokenAdminFilters.ContainsKey(type) ? tokenAdminFilters[type] : null;
        }

        public SetFilters GetAppFilters(EntitySetType type)
        {
            return tokenAppFilters.ContainsKey(type) ? tokenAppFilters[type] : null;
        }
    }
}
