using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class Utils
    {
        public static string GetJson(object obj)
        {
            Type type = obj.GetType();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
            var strm = new System.IO.MemoryStream();
            ser.WriteObject(strm, obj);
            string json = System.Text.Encoding.UTF8.GetString(strm.ToArray());
            return json;
        }

        public static string GetDynamicJson(object obj)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            return ser.Serialize(obj);
        }

        public static string GetHtmlRolePath(string strpath)
        {
            string[] pnodes = strpath.Split('.');
            string path = "";
            foreach (var pn in pnodes)
                path += (path == "" ? "" : "<span class='ion-star path-separator'></span>") + "<span class='path-node'>" + pn + "</span>";
            return path;
        }

    }
}
