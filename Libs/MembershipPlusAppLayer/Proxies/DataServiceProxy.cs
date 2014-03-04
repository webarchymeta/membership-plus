using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using Archymeta.Web.Search.Proxies;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer.Proxies
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DataServiceProxy : IDataServiceProxy
    {
        public string GetSetInfo(string sourceId, string set)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            EntitySetType type;
            if (Enum.TryParse<EntitySetType>(sobj["set"], out type))
            {
                string filter = null;
                if (sobj.ContainsKey("setFilter"))
                    filter = sobj["setFilter"];
                switch (type)
                {
                    case EntitySetType.User:
                        {
                            UserServiceProxy svc = new UserServiceProxy();
                            var si = svc.GetSetInfo(ApplicationContext.ClientContext, filter);
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            string json = ser.Serialize(new { EntityCount = si.EntityCount, Sorters = si.Sorters });
                            return json;
                        }
                    case EntitySetType.Role:
                        {
                            RoleServiceProxy svc = new RoleServiceProxy();
                            var si = svc.GetSetInfo(ApplicationContext.ClientContext, filter);
                            JavaScriptSerializer ser = new JavaScriptSerializer();
                            string json = ser.Serialize(new { EntityCount = si.EntityCount, Sorters = si.Sorters });
                            return json;
                        }
                }
            }
            return null;
        }

        public string GetNextSorterOps(string sourceId, string set, string sorters)
        {
            EntitySetType type;
            if (Enum.TryParse<EntitySetType>(set, out type))
            {
                switch (type)
                {
                    case EntitySetType.User:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(List<QToken>));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(TokenOptions));
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(sorters);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _sorters = ser1.ReadObject(strm) as List<QToken>;
                            UserServiceProxy svc = new UserServiceProxy();
                            var result = svc.GetNextSorterOps(ApplicationContext.ClientContext, _sorters);
                            strm = new System.IO.MemoryStream();
                            ser2.WriteObject(strm, result);
                            string json = System.Text.Encoding.UTF8.GetString(strm.ToArray());
                            return json;
                        }
                    case EntitySetType.Role:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(List<QToken>));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(TokenOptions));
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(sorters);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _sorters = ser1.ReadObject(strm) as List<QToken>;
                            RoleServiceProxy svc = new RoleServiceProxy();
                            var result = svc.GetNextSorterOps(ApplicationContext.ClientContext, _sorters);
                            strm = new System.IO.MemoryStream();
                            ser2.WriteObject(strm, result);
                            string json = System.Text.Encoding.UTF8.GetString(strm.ToArray());
                            return json;
                        }
                }
            }
            return null;
        }

        public string GetNextFilterOps(string sourceId, string set, string qexpr)
        {
            EntitySetType type;
            if (Enum.TryParse<EntitySetType>(set, out type))
            {
                switch (type)
                {
                    case EntitySetType.User:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(TokenOptions));
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
                            UserServiceProxy svc = new UserServiceProxy();
                            var result = svc.GetNextFilterOps(ApplicationContext.ClientContext, _qexpr, "");
                            strm = new System.IO.MemoryStream();
                            ser2.WriteObject(strm, result);
                            string json = System.Text.Encoding.UTF8.GetString(strm.ToArray());
                            return json;
                        }
                    case EntitySetType.Role:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(TokenOptions));
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
                            RoleServiceProxy svc = new RoleServiceProxy();
                            var result = svc.GetNextFilterOps(ApplicationContext.ClientContext, _qexpr, "");
                            strm = new System.IO.MemoryStream();
                            ser2.WriteObject(strm, result);
                            string json = System.Text.Encoding.UTF8.GetString(strm.ToArray());
                            return json;
                        }
                }
            }
            return null;
        }

        public string NextPageBlock(string sourceId, string set, string qexpr, string prevlast)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            EntitySetType type;
            if (Enum.TryParse<EntitySetType>(sobj["set"], out type))
            {
                switch (type)
                {
                    case EntitySetType.User:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(User));
                            DataContractJsonSerializer ser3 = new DataContractJsonSerializer(typeof(UserPageBlock));
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
                            UserServiceProxy svc = new UserServiceProxy();
                            UserSet _set = new UserSet();
                            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
                            _set.PageSize_ = int.Parse(sobj["pageSize"]);
                            if (sobj.ContainsKey("setFilter"))
                                _set.SetFilter = sobj["setFilter"];
                            User _prevlast = null;
                            if (!string.IsNullOrEmpty(prevlast))
                            {
                                strm = new System.IO.MemoryStream();
                                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                                strm.Write(sbf, 0, sbf.Length);
                                strm.Position = 0;
                                _prevlast = ser2.ReadObject(strm) as User;
                            }
                            var result = svc.NextPageBlock(ApplicationContext.ClientContext, _set, _qexpr, _prevlast);
                            strm = new System.IO.MemoryStream();
                            ser3.WriteObject(strm, result);
                            string json = System.Text.Encoding.UTF8.GetString(strm.ToArray());
                            return json;
                        }
                    case EntitySetType.Role:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(Role));
                            DataContractJsonSerializer ser3 = new DataContractJsonSerializer(typeof(RolePageBlock));
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
                            RoleServiceProxy svc = new RoleServiceProxy();
                            RoleSet _set = new RoleSet();
                            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
                            _set.PageSize_ = int.Parse(sobj["pageSize"]);
                            if (sobj.ContainsKey("setFilter"))
                                _set.SetFilter = sobj["setFilter"];
                            Role _prevlast = null;
                            if (!string.IsNullOrEmpty(prevlast))
                            {
                                strm = new System.IO.MemoryStream();
                                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                                strm.Write(sbf, 0, sbf.Length);
                                strm.Position = 0;
                                _prevlast = ser2.ReadObject(strm) as Role;
                            }
                            var result = svc.NextPageBlock(ApplicationContext.ClientContext, _set, _qexpr, _prevlast);
                            strm = new System.IO.MemoryStream();
                            ser3.WriteObject(strm, result);
                            string json = System.Text.Encoding.UTF8.GetString(strm.ToArray());
                            return json;
                        }
                }
            }
            return null;
        }

        public string GetPageItems(string sourceId, string set, string qexpr, string prevlast)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            dynamic sobj = jser.DeserializeObject(set) as dynamic;
            EntitySetType type;
            if (Enum.TryParse<EntitySetType>(sobj["set"], out type))
            {
                switch (type)
                {
                    case EntitySetType.User:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(User));
                            var ser3 = new JavaScriptSerializer();
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
                            UserServiceProxy svc = new UserServiceProxy();
                            UserSet _set = new UserSet();
                            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
                            _set.PageSize_ = int.Parse(sobj["pageSize"]);
                            if (sobj.ContainsKey("setFilter"))
                                _set.SetFilter = sobj["setFilter"];
                            User _prevlast = null;
                            if (!string.IsNullOrEmpty(prevlast))
                            {
                                strm = new System.IO.MemoryStream();
                                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                                strm.Write(sbf, 0, sbf.Length);
                                strm.Position = 0;
                                _prevlast = ser2.ReadObject(strm) as User;
                            }
                            var result = svc.GetPageItems(ApplicationContext.ClientContext, _set, _qexpr, _prevlast);
                            var ar = new List<dynamic>();
                            foreach (var e in result)
                            {
                                ar.Add(new { Id = e.ID.ToString(), DistinctString = e.DistinctString });
                            }
                            string json = ser3.Serialize(ar);
                            return json;
                        }
                    case EntitySetType.Role:
                        {
                            DataContractJsonSerializer ser1 = new DataContractJsonSerializer(typeof(QueryExpresion));
                            DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(Role));
                            var ser3 = new JavaScriptSerializer();
                            System.IO.MemoryStream strm = new System.IO.MemoryStream();
                            byte[] sbf = System.Text.Encoding.UTF8.GetBytes(qexpr);
                            strm.Write(sbf, 0, sbf.Length);
                            strm.Position = 0;
                            var _qexpr = ser1.ReadObject(strm) as QueryExpresion;
                            RoleServiceProxy svc = new RoleServiceProxy();
                            RoleSet _set = new RoleSet();
                            _set.PageBlockSize = int.Parse(sobj["pageBlockSize"]);
                            _set.PageSize_ = int.Parse(sobj["pageSize"]);
                            if (sobj.ContainsKey("setFilter"))
                                _set.SetFilter = sobj["setFilter"];
                            Role _prevlast = null;
                            if (!string.IsNullOrEmpty(prevlast))
                            {
                                strm = new System.IO.MemoryStream();
                                sbf = System.Text.Encoding.UTF8.GetBytes(prevlast);
                                strm.Write(sbf, 0, sbf.Length);
                                strm.Position = 0;
                                _prevlast = ser2.ReadObject(strm) as Role;
                            }
                            var result = svc.GetPageItems(ApplicationContext.ClientContext, _set, _qexpr, _prevlast);
                            var ar = new List<dynamic>();
                            foreach (var e in result)
                            {
                                ar.Add(new { Id = e.ID.ToString(), DistinctString = e.DistinctString });
                            }
                            string json = ser3.Serialize(ar);
                            return json;
                        }
                }
            }
            return null;
        }
    }
}
