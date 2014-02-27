using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Dictionary<EntitySetType, Dictionary<string, TokenMapRec>> dic = new Dictionary<EntitySetType, Dictionary<string, TokenMapRec>>();

        public QueryCustomization(XmlNode node)
        {
            XmlNode xcommon = node.SelectSingleNode("set[@name='']");
            if (xcommon != null)
            {
                Dictionary<string, TokenMapRec> maps = new Dictionary<string, TokenMapRec>();
                dic.Add(EntitySetType.DatabaseLevel, maps);
                var xmaps = xcommon.SelectNodes("map");
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
                    dic.Add(type, maps);
                    var xmaps = xset.SelectNodes("map");
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
                }
            }
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
                return dic.ContainsKey(type) ? dic[type] : null;
            }
        }
    }

}
