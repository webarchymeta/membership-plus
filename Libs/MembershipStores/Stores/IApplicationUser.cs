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
    public interface IApplicationUser : Microsoft.AspNet.Identity.IUser
    {
#if MemberPlus
        string Email { get; set; }
        bool HasIcon { get; set; }
#endif
        string AppMemberStatus { get; set; }
        string PasswordQuestion { get; set; }
        string PasswordAnswer { get; set; }
        ICollection<System.Security.Claims.Claim> Claims { get; }
        void UpdateInstance(User user);
    }
}
