using System;
using System.Text;
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Security.Principal;
#if MemberPlus
using CryptoGateway.RDB.Data.MembershipPlus;
#else
using CryptoGateway.RDB.Data.AspNetMember;
#endif
using Archymeta.Web.Security.Resources;

namespace Archymeta.Web.Security
{

    public enum AuthFailedTypes
    {
        Unknown,
        UnknownUser,
        InvalidCredential,
        ApprovalNeeded,
        UserAccountBlocked,
        MemberNotFound,
        MembershipBlocked,
        MembershipFrozen,
        MembershipRecovered,
        ActionTip
    }

    public class AuthFailedEventArg
    {
        public AuthFailedTypes FailType
        {
            get { return _failType; }
            set { _failType = value; }
        }
        private AuthFailedTypes _failType = AuthFailedTypes.Unknown;

        public string FailMessage
        {
            get;
            set;
        }
    }

    //public static class IdentityExExtensions
    //{
    //    public static string GetUserIdEx(this IIdentity identity)
    //    {
    //        if (identity is ClaimsIdentityEx)
    //            return (identity as ClaimsIdentityEx).ID;
    //        else
    //            return null;
    //    }
    //}

    public class UserManagerEx<TUser> : UserManager<TUser> where TUser : User, IApplicationUser, new()
    {
        public CallContext Cctx
        {
            get { return _cctx; }
            set { _cctx = value; }
        }
        private CallContext _cctx;

        public Application_ App
        {
            get { return app; }
            set { app = value; }
        }
        private Application_ app;

        public Action<AuthFailedEventArg> ExternalErrorsHandler = null;

        public UserManagerEx(IUserStore<TUser> store)
            : base(store)
        {
            this.PasswordHasher = store as UserStore<TUser>;
            this.PasswordValidator = store as UserStore<TUser>;
            this.UserValidator = new UserValidator<TUser>();
        }

        public UserManagerEx(IUserStore<TUser> store, CallContext clientContext, Application_ app)
            : this(store)
        {
            this.app = app;
            _cctx = clientContext;
        }

        private void ErrorsHandler(string userName, AuthFailedEventArg error, bool cache = true)
        {
            if (ExternalErrorsHandler != null)
            {
                ExternalErrorsHandler(error);
                if (cache && HttpContext.Current != null)
                    ExternalErrorsHandler(new AuthFailedEventArg
                    {
                        FailType = AuthFailedTypes.ActionTip,
                        FailMessage = string.Format(ResourceUtils.GetString("8b3c7a0358df5b19b82260b843718251", "Try to login after {0} seconds."), (Store as UserStore<TUser>).PasswordAttemptWindow)
                    });
            }
            if (HttpContext.Current != null)
            {
                if (cache)
                {
                    string cacheKey = "userLoginState:" + userName;
                    int cTime = (Store as UserStore<TUser>).PasswordAttemptWindow;
                    HttpContext.Current.Cache.Add(cacheKey, error, null, DateTime.Now.AddSeconds(cTime),
                                                  System.Web.Caching.Cache.NoSlidingExpiration,
                                                  System.Web.Caching.CacheItemPriority.Normal, null);
                }
            }
        }

        public override async Task<IdentityResult> CreateAsync(TUser user, string password)
        {
            try
            {
                var validInfo = await this.UserValidator.ValidateAsync(user);
                if (validInfo.Succeeded)
                {
                    user.Password = PasswordHasher.HashPassword(password);
                    await Store.CreateAsync(user);
                    return IdentityResult.Success;
                }
                else
                {
                    return validInfo;
                }
            }
            catch (Exception e)
            {
                List<string> errs = new List<string>();
                while (e != null)
                {
                    errs.Add(e.Message);
                    e = e.InnerException;
                }
                return IdentityResult.Failed(errs.ToArray());
            }
        }

        public override async Task<TUser> FindAsync(string userName, string password)
        {
            if (HttpContext.Current != null)
            {
                string cacheKey = "userLoginState:" + userName;
                var error = HttpContext.Current.Cache[cacheKey] as AuthFailedEventArg;
                if (error != null)
                {
                    ErrorsHandler(userName, error);
                    return null;
                }
            }
            CallContext cctx = _cctx.CreateCopy();
            UserServiceProxy usvc = new UserServiceProxy();
            UserSet us = new UserSet();
            var lu = await usvc.LoadEntityByNatureAsync(cctx, userName);
            if (lu == null || lu.Count == 0)
            {
                var err = new AuthFailedEventArg
                {
                    FailType = AuthFailedTypes.UnknownUser,
                    FailMessage = ResourceUtils.GetString("3488820581565e9098c46152335ebb24", "Your don't have an account in the present system, please register!")
                };
                ErrorsHandler(userName, err);
                return null;
            }
            var u = lu[0];
            if (!u.IsApproved)
            {
                var err = new AuthFailedEventArg
                {
                    FailType = AuthFailedTypes.ApprovalNeeded,
                    FailMessage = ResourceUtils.GetString("3bdf31486d76404d69c73b90c790f9be", "Your account is pending for approval, please wait!")
                };
                ErrorsHandler(userName, err);
                return null;
            }
            if (u.Status != us.StatusValues[0])
            {
                var err = new AuthFailedEventArg
                {
                    FailType = AuthFailedTypes.UserAccountBlocked,
                    FailMessage = string.Format(ResourceUtils.GetString("0bcd70b0b005df9491a0623280ee1f4d", "Your account is in the state of being [{0}], please contact an administrator!"), u.Status)
                };
                ErrorsHandler(userName, err);
                return null;
            }
            UserAppMemberSet membs = new UserAppMemberSet();
            UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
            var memb = await mbsvc.LoadEntityByKeyAsync(cctx, app.ID, u.ID);
            if (memb == null)
            {
                var err = new AuthFailedEventArg
                {
                    FailType = AuthFailedTypes.MemberNotFound,
                    FailMessage = string.Format(ResourceUtils.GetString("d084974602e8940a962aad7d00bf7b3e", "You are not currently a member of \"{0}\", please register."), string.IsNullOrEmpty(app.DisplayName) ? app.Name : app.DisplayName)
                };
                ErrorsHandler(userName, err);
                return null;
            }
            if (memb.MemberStatus != membs.MemberStatusValues[0])
            {
                if (memb.MemberStatus != membs.MemberStatusValues[3])
                {
                    var err = new AuthFailedEventArg
                    {
                        FailType = AuthFailedTypes.MembershipBlocked,
                        FailMessage = string.Format(ResourceUtils.GetString("3508707fb8263c95b4c022dd0468235b", "Your membership in \"{0}\" is in the state of being [{1}], please contact an administrator!"), string.IsNullOrEmpty(app.DisplayName) ? app.Name : app.DisplayName, memb.MemberStatus)
                    };
                    ErrorsHandler(userName, err);
                    return null;
                }
                else
                {
                    var windowStart = u.FailedPasswordAttemptWindowStart.HasValue ? u.FailedPasswordAttemptWindowStart.Value : DateTime.MinValue;
                    DateTime windowEnd = windowStart.AddSeconds((Store as UserStore<TUser>).PasswordAttemptWindow);
                    if (DateTime.UtcNow <= windowEnd)
                    {
                        var err = new AuthFailedEventArg
                        {
                            FailType = AuthFailedTypes.MembershipFrozen,
                            FailMessage = string.Format(ResourceUtils.GetString("99529364b5dfda1d15a5859cd49c5a7c", "Maximum login attemps for \"{0}\" exceeded, please try again later!"), string.IsNullOrEmpty(app.DisplayName) ? app.Name : app.DisplayName)
                        };
                        ErrorsHandler(userName, err, false);
                        return null;
                    }
                    else
                    {
                        memb.MemberStatus = membs.MemberStatusValues[0];
                        memb.IsMemberStatusModified = true;
                        memb.LastStatusChange = DateTime.UtcNow;
                        memb.IsLastStatusChangeModified = true;
                        await mbsvc.AddOrUpdateEntitiesAsync(cctx, membs, new UserAppMember[] { memb });
                        var err = new AuthFailedEventArg
                        {
                            FailType = AuthFailedTypes.MembershipRecovered,
                            FailMessage = ResourceUtils.GetString("8cdaed0e2a0dd2e31c4960412351d4b5", "Your membership status is restored, please try again!")
                        };
                        if (u.FailedPasswordAttemptCount != 0)
                        {
                            u.FailedPasswordAttemptCount = 0;
                            usvc.EnqueueNewOrUpdateEntities(cctx, us, new User[] { u });
                        }
                        ErrorsHandler(userName, err, false);
                        return null;
                    }
                }
            }
            TUser user = new TUser();
            user.UpdateInstance(u);
            var found = await base.FindAsync(userName, password);
            if (found == null)
            {
                await (Store as UserStore<TUser>).UpdateFailureCountAsync(cctx, user, "password");
                var err = new AuthFailedEventArg
                {
                    FailType = AuthFailedTypes.InvalidCredential,
                    FailMessage = ResourceUtils.GetString("3a2a06b3a1f05cde765219211bf2e9be", "Invalid username or password.")
                };
                ErrorsHandler(userName, err, false);
            }
            else
            {
                u.LastLoginDate = DateTime.UtcNow;
                u.IsLastLoginDateModified = true;
                usvc.EnqueueNewOrUpdateEntities(cctx, new UserSet(), new User[] { u });
                memb.LastActivityDate = u.LastLoginDate;
                memb.IsLastActivityDateModified = true;
                mbsvc.EnqueueNewOrUpdateEntities(cctx, membs, new UserAppMember[] { memb });
            }
            return found;
        }

        public override async Task<ClaimsIdentity> CreateIdentityAsync(TUser user, string authenticationType)
        {
            var identity = new ClaimsIdentity(user.Claims, authenticationType);
            return identity;
        }
    }
}
