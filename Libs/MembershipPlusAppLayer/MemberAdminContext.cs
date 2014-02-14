using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using Archymeta.Web.Security.Resources;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class MemberAdminContext
    {
        internal static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext.CreateCopy();
            }
        }
    }
}
