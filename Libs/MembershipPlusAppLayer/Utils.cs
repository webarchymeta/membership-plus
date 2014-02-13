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
    }
}
