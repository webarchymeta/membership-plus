using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Archymeta.Web.Search.Proxies
{
    [ServiceContract(Namespace = "http://relationaldb.archymeta.com/DynamicFileSystem/", SessionMode = SessionMode.Allowed)]
    public interface IDataServiceProxy
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/GetSetInfo")]
        string GetSetInfo(string sourceId, string set);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/GetNextSorterOps")]
        string GetNextSorterOps(string sourceId, string set, string sorters);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/GetNextFilterOps")]
        string GetNextFilterOps(string sourceId, string set, string qexpr);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/NextPageBlock")]
        string NextPageBlock(string sourceId, string set, string qexpr, string prevlast);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/GetPageItems")]
        string GetPageItems(string sourceId, string set, string qexpr, string prevlast);
    }
}
