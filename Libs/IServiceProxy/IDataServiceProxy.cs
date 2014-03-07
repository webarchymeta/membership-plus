using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
#if SUPPORT_ASYNC
using System.Threading.Tasks;
#endif
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
#if SUPPORT_ASYNC
        Task<string>
#else
        string 
#endif       
        GetSetInfo(string sourceId, string set);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/GetNextSorterOps")]
#if SUPPORT_ASYNC
        Task<string>
#else
        string 
#endif
        GetNextSorterOps(string sourceId, string set, string sorters);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/GetNextFilterOps")]
#if SUPPORT_ASYNC
        Task<string>
#else
        string 
#endif       
        GetNextFilterOps(string sourceId, string set, string qexpr);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/NextPageBlock")]
#if SUPPORT_ASYNC
        Task<string>
#else
        string 
#endif
        NextPageBlock(string sourceId, string set, string qexpr, string prevlast);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "/GetPageItems")]
#if SUPPORT_ASYNC
        Task<string>
#else
        string 
#endif
        GetPageItems(string sourceId, string set, string qexpr, string prevlast);
    }
}
