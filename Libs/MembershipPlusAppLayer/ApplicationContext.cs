using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class ApplicationContext
    {
        internal static object SyncRoot = new object();

        public static CallContext ClientContext
        {
            get
            {
                CallContext c;
                lock(SyncRoot)
                {
                    if (_clientContext != null)
                    {
                        c = _clientContext.CreateCopy();
                        c.AcceptLanguages = HttpContext.Current.Request.Headers["Accept-Language"];
                    }
                    else  
                        c = null; 
                }
                return c;
            }
            set { _clientContext = value; }
        }
        private static CallContext _clientContext = null;

        public static Application_ App
        {
            get;
            set;
        }
    }
}
