using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class ApplicationContext
    {
        public static CallContext ClientContext
        {
            get;
            set;
        }

        public static Application_ App
        {
            get;
            set;
        }
    }
}
