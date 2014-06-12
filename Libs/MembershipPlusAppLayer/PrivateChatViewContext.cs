using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using Archymeta.Web.Security.Resources;
using Archymeta.Web.Security;


namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class PrivateChatViewContext
    {
        internal static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext;
            }
        }

        public static async Task<string> GetMessages(string userId, string set, string qexpr, string prevlast)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(ShortMessage));
            var ser3 = new JavaScriptSerializer();
            System.IO.MemoryStream strm = new System.IO.MemoryStream();
            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
            strm.Write(sbf, 0, sbf.Length);
            strm.Position = 0;
            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
            var svc = new ShortMessageServiceProxy();
            var _set = new ShortMessageSet();
            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
            _set.PageSize_ = int.Parse(sobj["pageSize"]);
            if (!sobj.ContainsKey("setFilter"))
                throw new Exception("The page is not properly parameterized!");
            else
            {
                string filter = sobj["setFilter"];
                if (filter.EndsWith("&& ToID is not null && GroupID is null && ( ToID == \"{0}\" || FromID == \"{0}\" )"))
                {
                    filter = string.Format(filter, userId);
                    _set.SetFilter = filter;
                }
                else
                    throw new Exception("The page is not properly parameterized!");
            }
            ShortMessage _prevlast = null;
            if (!string.IsNullOrEmpty(prevlast))
            {
                strm = new System.IO.MemoryStream();
                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                strm.Write(sbf, 0, sbf.Length);
                strm.Position = 0;
                _prevlast = ser2.ReadObject(strm) as ShortMessage;
            }
            var result = await svc.GetPageItemsAsync(Cntx, _set, _qexpr, _prevlast);
            var ar = new List<dynamic>();
            foreach (var e in result)
            {
                ar.Add(new { data = e });
            }
            string json = ser3.Serialize(ar);
            return json;
        }
    }
}
