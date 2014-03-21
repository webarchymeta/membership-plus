using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
#if MemberPlus
using CryptoGateway.RDB.Data.MembershipPlus;
#else
using CryptoGateway.RDB.Data.AspNetMember;
#endif

namespace Archymeta.Web.Security
{
    public class ApplicationUser : User, IApplicationUser, IIdentity
    {
        string IUser<string>.Id
        {
            get { return ID; }
        }

        string IUser<string>.UserName
        {
            get
            {
                return Username;
            }
            set
            {
                Username = value;
            }
        }

        string IIdentity.AuthenticationType
        {
            get { return DefaultAuthenticationTypes.ApplicationCookie; }
        }

        string IIdentity.Name
        {
            get { return Username; }
        }

        public bool IsAuthenticated
        {
            get;
            set;
        }

        public string AppMemberStatus
        {
            get;
            set;
        }

#if MemberPlus
        public string Email
        {
            get;
            set;
        }

        public bool HasIcon
        {
            get;
            set;
        }
#endif

        public ICollection<Claim> Claims
        {
            get
            {
                if (_claims.Count == 0)
                {
                    _claims.Add(UserStore<ApplicationUser>.CreateClaim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", UserStore<ApplicationUser>.NameProviderId));
                    _claims.Add(UserStore<ApplicationUser>.CreateClaim(Microsoft.IdentityModel.Claims.ClaimTypes.NameIdentifier, ID));
                    _claims.Add(UserStore<ApplicationUser>.CreateClaim(Microsoft.IdentityModel.Claims.ClaimTypes.Name, Username));
                    if (!string.IsNullOrEmpty(Email))
                        _claims.Add(UserStore<ApplicationUser>.CreateClaim(Microsoft.IdentityModel.Claims.ClaimTypes.Email, Email));
                }
                return _claims;
            }
        }
        private List<Claim> _claims = new List<Claim>();

        public void UpdateInstance(User u)
        {
            IsPersisted = false;
            User.MergeChanges(u, this);
            IsPersisted = u.IsPersisted;
        }
    }

    public class ApplicationRole : Role, IApplicationRole
    {
        public string Id
        {
            get { return ID.ToString(); }
        }

        public string Name
        {
            get;
            set;
        }

        public void UpdateInstance(Role r)
        {
            IsPersisted = false;
            Role.MergeChanges(r, this);
            IsPersisted = r.IsPersisted;
        }
    }
}
