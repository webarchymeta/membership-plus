using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if MemberPlus
using CryptoGateway.RDB.Data.MembershipPlus;
#else
using CryptoGateway.RDB.Data.AspNetMember;
#endif

namespace Archymeta.Web.Security
{
    public interface IApplicationRole : Microsoft.AspNet.Identity.IRole
    {
        void UpdateInstance(Role r);
    }
}
