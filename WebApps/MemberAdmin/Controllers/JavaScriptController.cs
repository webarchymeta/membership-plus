using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.MembershipPlus.AppLayer.Configuration;

namespace MemberAdminMvc5.Controllers
{
    public class JavaScriptController : BaseController
    {
        private static QueryCustomization QueryTokenMap = null;

        public JavaScriptController()
        {
            if (QueryTokenMap == null)
            {
                QueryTokenMap = ConfigurationManager.GetSection("query/customization") as QueryCustomization;
            }
        }

        [HttpGet]
        public ActionResult QueryCustomization()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"
function tokenNameMap(tk, entity, isquery) {
    map" + EntitySetType.DatabaseLevel + @"Tokens(tk, isquery);
    switch (entity) {");
            foreach (var _etype in Enum.GetValues(typeof(EntitySetType)))
            {
                var etype = (EntitySetType)_etype;
                if (etype == EntitySetType.Unknown || etype == EntitySetType.DatabaseLevel)
                    continue;
                if (QueryTokenMap[etype] != null)
                {
                    sb.Append(@"
        case '" + etype + @"':");
                    switch (etype)
                    {
                        case EntitySetType.User:
                            {
                                sb.Append(@"
            {
                if (!isquery) {
                    if (tk.TkName.indexOf('Password') == -1) {
                        map" + etype + @"Tokens(tk, isquery);
                    } else {
                        return false;  // the password related attributes are excluded from sorting;
                    }
                } else {
                    if (tk.TkName.indexOf('Password') == -1 || tk.TkName.indexOf('Password') != -1 && tk.TkName.indexOf('Failed') != -1) {
                        map" + etype + @"Tokens(tk, isquery);
                    } else {
                        return false;  //  the password related attributes are excluded from querying;
                    }
                }
            }");
                            }
                            break;
                        default:
                            {
                                sb.Append(@"
            map" + etype + @"Tokens(tk, isquery);");
                            }
                            break;
                    }
                    sb.Append(@"
            break;");
                }
            }
            sb.Append(@"
    }
    return true;
}
");
            foreach (var _etype in Enum.GetValues(typeof(EntitySetType)))
            {
                var etype = (EntitySetType)_etype;
                if (etype == EntitySetType.Unknown)
                    continue;
                Dictionary<string, TokenMapRec> maps = QueryTokenMap[etype];
                if (maps != null)
                {
                    sb.Append(@"
function map" + etype + @"Tokens(tk, isquery) {
    switch (tk.TkName) {");
                    foreach (var kvp in maps)
                    {
                        sb.Append(@"
        case """ + kvp.Key + @""":
            tk.DisplayAs = '" + (kvp.Value.ToID != null ? ResourceUtils.GetString(StoreTypes.QueryResources, kvp.Value.ToID, kvp.Value.ToName) : kvp.Value.ToName) + @"';
            break;");
                    }
                    sb.Append(@"
        default:
            break;
    }
}
");
                }
            }
            return ReturnJavascript(sb.ToString());
        }

	}
}