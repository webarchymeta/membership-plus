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
        public ActionResult QueryAdminCustomization(string src)
        {
            if (QueryTokenMap == null || !QueryTokenMap.ConfigExists)
                return new HttpStatusCodeResult(404, "Not Found");
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(src))
                src = ConfigurationManager.AppSettings["DefaultDataSource"];
            _queryCustomization(sb, src, QueryTokenMap.GetAdminFilters);
            return ReturnJavascript(sb.ToString());
        }

        [HttpGet]
        public ActionResult QueryCustomization(string src)
        {
            if (QueryTokenMap == null || !QueryTokenMap.ConfigExists)
                return new HttpStatusCodeResult(404, "Not Found");
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(src))
                src = ConfigurationManager.AppSettings["DefaultDataSource"];
            _queryCustomization(sb, src, QueryTokenMap.GetAppFilters);
            return ReturnJavascript(sb.ToString());
        }

        private void _queryCustomization(StringBuilder sb, string src, Func<string, string, SetFilters> getfilters)
        {
            sb.Append(@"
function tokenNameMap(tk, entity, isquery) {");
            if (QueryTokenMap.GlobalTable != null)
            {
                sb.Append(@"
    mapCommonTokens(tk, isquery);");
            }
            var dic = QueryTokenMap.GetMaps(src);
            if (dic != null)
            {
                sb.Append(@"
    switch (entity) {");
                foreach (var kvp in dic)
                {
                    sb.Append(@"
        case '" + kvp.Key + @"':
            {");
                    sb.Append(@"
                map" + kvp.Key + @"Tokens(tk, isquery);");
                    var appfilters = getfilters(src, kvp.Key);
                    if (appfilters != null && appfilters.Filters.Length > 0)
                        GetFilterSection(appfilters, sb);
                    sb.Append(@"
            }
            break;");
                }
                sb.Append(@"
    }");
            }
            sb.Append(@"
    return true;
}
");
            GetMapFunction(src, sb);
        }

        private void GetFilterSection(SetFilters setFilters, StringBuilder sb)
        {
            for (int i = 0; i < setFilters.Filters.Length; i++)
            {
                var filter = setFilters.Filters[i];
                sb.Append(@"
                " + (i > 0 ? "else " : "") + @"if (");
                switch(filter.Target)
                {
                    case TokenMatchTarget.Sorting:
                        sb.Append("!isquery && (");
                        break;
                    case TokenMatchTarget.Filtering:
                        sb.Append("isquery && (");
                        break;
                }
                switch(filter.Kind)
                {
                    case TokenMatchKind.Equals:
                        if (!filter.IsCaseSensitive)
                            sb.Append("tk.TkName.toLowerCase() == '" + filter.FilterExpr + "'.toLowerCase()");
                        else
                            sb.Append("tk.TkName. == '" + filter.FilterExpr + "'");
                        break;
                    case TokenMatchKind.Contains:
                        if (!filter.IsCaseSensitive)
                            sb.Append("tk.TkName.toLowerCase().indexOf('" + filter.FilterExpr + "'.toLowerCase()) != -1");
                        else
                            sb.Append("tk.TkName.indexOf('" + filter.FilterExpr + "') != -1");
                        break;
                    case TokenMatchKind.StartsWith:
                        if (!filter.IsCaseSensitive)
                            sb.Append("tk.TkName.toLowerCase().indexOf('" + filter.FilterExpr + "'.toLowerCase()) == 0");
                        else
                            sb.Append("tk.TkName.indexOf('" + filter.FilterExpr + "') == 0");
                        break;
                    case TokenMatchKind.EndsWith:
                        if (!filter.IsCaseSensitive)
                            sb.Append("tk.TkName.toLowerCase().indexOf('" + filter.FilterExpr + "'.toLowerCase()) == tk.TkName.length - '" + filter.FilterExpr + "'.length");
                        else
                            sb.Append("tk.TkName.indexOf('" + filter.FilterExpr + "') == tk.TkName.length - '" + filter.FilterExpr + "'.length");
                        break;
                    case TokenMatchKind.Expression:
                        sb.Append(string.Format(filter.FilterExpr, "tk.TkName"));
                        break;
                    case TokenMatchKind.Regex:
                        sb.Append("tk.TkName.match(" + filter.FilterExpr + ")");
                        break;
                }
                switch (filter.Target)
                {
                    case TokenMatchTarget.Sorting:
                    case TokenMatchTarget.Filtering:
                        sb.Append(")");
                        break;
                }
                sb.Append(@")
                    return " + (filter.IsAllowed ? "true" : "false") + @";");
            }
            sb.Append(@"
                else
                    return " + (setFilters.AllowImplied ? "true" : "false") + @";");
        }

        private void GetMapFunction(string src, StringBuilder sb)
        {
            if (QueryTokenMap.GlobalTable != null)
            {
                sb.Append(@"
function mapCommonTokens(tk, isquery) {
    switch (tk.TkName) {");
                foreach (var kvp in QueryTokenMap.GlobalTable.tokenMaps)
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
            var dic = QueryTokenMap.GetMaps(src);
            if (dic != null)
            {
                foreach (var dickvp in dic)
                {
                    sb.Append(@"
function map" + dickvp.Key + @"Tokens(tk, isquery) {
    switch (tk.TkName) {");
                    foreach (var kvp in dickvp.Value)
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
        }
    }
}