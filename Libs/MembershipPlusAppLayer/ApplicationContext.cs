using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class ApplicationContext
    {
        public const int ChatAssocTypeId = 12;
        public const int PrivateChatNoticeTypeId = 3;

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
                        //(HttpContext.Current may not be available in some application environment
                        if (HttpContext.Current != null)
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

        public static int OnlineUserInactiveTime
        {
            get
            {
                string val = ConfigurationManager.AppSettings["OnlineUserInactiveTimeInMinutes"] == null ? "20" : ConfigurationManager.AppSettings["OnlineUserInactiveTimeInMinutes"];
                int minutes;
                if (!int.TryParse(val, out minutes))
                    minutes = 20;
                return minutes;
            }
        }

        public static Application_ App
        {
            get;
            set;
        }
    }
}
