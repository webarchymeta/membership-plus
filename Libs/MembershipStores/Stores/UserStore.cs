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
using Microsoft.AspNet.Identity;
#if MemberPlus
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.Security.Resources;
#else
using CryptoGateway.RDB.Data.AspNetMember;
#endif


namespace Archymeta.Web.Security
{
    public class CustomClaims
    {
        public static string HasIcon
        {
            get { return Microsoft.IdentityModel.Claims.ClaimTypes.ClaimType2009Namespace + "/Archymeta/HasIcon"; }
        }
    }

    public class UserValidator<TUser> : IIdentityValidator<TUser>
    {
        public async Task<IdentityResult> ValidateAsync(TUser user)
        {
            //temp ...
            return IdentityResult.Success;
        }
    }

    /* other interfaces: IUserLoginStore<TUser>, IUserSecurityStampStore<TUser>, */
    public class UserStore<TUser> : IUserStore<TUser>, IUserPasswordStore<TUser>, IUserRoleStore<TUser>, IUserClaimStore<TUser>, IPasswordHasher, IIdentityValidator<string>, IDisposable where TUser : User, IApplicationUser, new()
    {
        public const string NameProviderId = "Archymeta Membership Service";

#if TEST
        public CallContext Cctx
        {
            get { return _cctx; }
        }
#endif
        private CallContext _cctx;
        private Application_ app;

        private MachineKeySection machineKey
        {
            get
            {
                if (_machineKey == null)
                {
                    Configuration cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
                    _machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");
                }
                return _machineKey;
            }
        }
        private MachineKeySection _machineKey = null;

        public bool WriteExceptionsToEventLog
        {
            get
            {
                if (_writeExceptionsToEventLog.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["WriteAuthExceptionsToEventLog"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _writeExceptionsToEventLog = bv;
                    else
                        _writeExceptionsToEventLog = false;
                }
                return _writeExceptionsToEventLog.Value;
            }
        }
        private bool? _writeExceptionsToEventLog = default(bool?);

        public bool RequiresUniqueEmail
        {
            get
            {
                if (_requiresUniqueEmail.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["RequiresUniqueUserEmail"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _requiresUniqueEmail = bv;
                    else
                        _requiresUniqueEmail = true;
                }
                return _requiresUniqueEmail.Value;
            }
        }
        private bool? _requiresUniqueEmail = default(bool?);

        public bool UserApprovedOnAddition
        {
            get
            {
                if (_userApprovedOnAddition.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["UserApprovedOnAddition"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _userApprovedOnAddition = bv;
                    else
                        _userApprovedOnAddition = true;
                }
                return _userApprovedOnAddition.Value;
            }
        }
        private bool? _userApprovedOnAddition = default(bool?);

        public bool DeleteUserMembershipOnly
        {
            get
            {
                if (_deleteUserMembershipOnly.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["DeleteUserMembershipOnly"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _deleteUserMembershipOnly = bv;
                    else
                        _deleteUserMembershipOnly = true;
                }
                return _deleteUserMembershipOnly.Value;
            }
        }
        private bool? _deleteUserMembershipOnly = default(bool?);

        /// <summary>
        /// In the current hierarchic role system, it determines whether or not to delete less specific roles than the current one and quit if more specific role exists. The default is true.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this to true will keep the data source clean since if a user is in a role, then he/she is in the more general roles above it, if any.
        /// But this could create more work if a role is deleted in which case the user will be removed from any more general implicit roles. If that is not intended, an administrator has to 
        /// re-assign a proper role within the deleted role family to the targeting user.
        /// </para>
        /// <para>
        /// Setting this to false could keep some historic roles in the data source. When a role is deleted in this case, the targeting user could still has certain less priviledged roles 
        /// inside the current role family. However, this may not be what is intended.
        /// </para>
        /// </remarks>
        public bool UserStoreAutoCleanupRoles
        {
            get
            {
                if (_userStoreAutoCleanupRoles.HasValue == false)
                {
                    bool bv;
                    string strv = ConfigurationManager.AppSettings["UserStoreAutoCleanupRoles"];
                    if (!string.IsNullOrEmpty(strv) && bool.TryParse(strv, out bv))
                        _userStoreAutoCleanupRoles = bv;
                    else
                        _userStoreAutoCleanupRoles = false;
                }
                return _userStoreAutoCleanupRoles.Value;
            }
        }
        private bool? _userStoreAutoCleanupRoles = default(bool?);

        public int PasswordAttemptWindow
        {
            get
            {
                if (!_passwordAttemptWindow.HasValue)
                {
                    int iv;
                    string strv = ConfigurationManager.AppSettings["PasswordAttemptWindow"];
                    if (!string.IsNullOrEmpty(strv) && int.TryParse(strv, out iv))
                        _passwordAttemptWindow = iv;
                    else
                        _passwordAttemptWindow = 20;
                }
                return _passwordAttemptWindow.Value;
            }
        }
        private int? _passwordAttemptWindow = default(int?);

        public int MaxInvalidPasswordAttempts
        {
            get
            {
                if (!_maxInvalidPasswordAttempts.HasValue)
                {
                    int iv;
                    string strv = ConfigurationManager.AppSettings["MaxInvalidPasswordAttempts"];
                    if (!string.IsNullOrEmpty(strv) && int.TryParse(strv, out iv))
                        _maxInvalidPasswordAttempts = iv;
                    else
                        _maxInvalidPasswordAttempts = 5;
                }
                return _maxInvalidPasswordAttempts.Value;
            }
        }
        private int? _maxInvalidPasswordAttempts = default(int?);

        public UserStore()
        {

        }

        public UserStore(CallContext clientContext, Application_ app)
        {
            _cctx = clientContext.CreateCopy();
            this.app = app;
        }

        public bool AutoSaveChanges { get; set; }
        public bool DisposeContext { get; set; }

        private Exception getException(string id, string err, Exception e = null)
        {
#if MemberPlus
            return new Exception(ResourceUtils.GetString(id, err), e);
#else
            return new Exception(err, e);
#endif
        }

        public async Task CreateAsync(TUser user)
        {
            CallContext cctx = _cctx.CreateCopy();
            try
            {
                UserSet us = new UserSet();
                UserAppMemberSet ums = new UserAppMemberSet();
                UserServiceProxy usvc = new UserServiceProxy();
                User udata = null;
                List<User> lu = await usvc.LoadEntityByNatureAsync(cctx, user.UserName);
                if (lu == null || lu.Count == 0)
                {
                    string id = user.Id;
                    if (id != null && await usvc.LoadEntityByKeyAsync(cctx, id) != null)
                        throw getException("fccd73d6fd2dec420710fd32dbe11527", "Duplicate user ID found.");
                    if (RequiresUniqueEmail)
                    {
                        var x = await GetUserNameByEmailAsync(user.Email);
                        if (!string.IsNullOrEmpty(x))
                            throw getException("22e97c1c4ffde9ff3283a88153d80e0a", "User email exists.");
                    }
                    DateTime createDate = DateTime.UtcNow;
                    if (id == null)
                    {
                        id = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        Guid guid;
                        if (!Guid.TryParse(id, out guid))
                            throw getException("7e4ec0669ebc17dc36870699c309c56b", "Invalid user ID found.");
                    }
                    udata = new User();
                    udata.IsPersisted = false;
                    udata.ID = id;
                    udata.Username = user.UserName;
                    udata.Password = (user as User).Password;
                    udata.PasswordFormat = "Hashed";
#if MemberPlus
#else
                    udata.Email = user.Email;
#endif
                    udata.PasswordQuestion = user.PasswordQuestion;
                    udata.PasswordAnswer = user.PasswordAnswer;
                    udata.IsApproved = UserApprovedOnAddition;
                    udata.CreateOn = createDate;
                    udata.LastPasswordChangedDate = createDate;
                    udata.FailedPasswordAttemptCount = 0;
                    udata.FailedPasswordAttemptWindowStart = createDate;
                    udata.FailedPasswordAnswerAttemptCount = 0;
                    udata.FailedPasswordAnswerAttemptWindowStart = createDate;
                    udata.Status = us.StatusValues[0];
                    UserAppMember memb = new UserAppMember();
                    memb.ApplicationID = app.ID;
                    memb.UserID = udata.ID;
                    memb.MemberStatus = ums.MemberStatusValues[0];
                    memb.LastStatusChange = createDate;
                    memb.LastActivityDate = createDate;
                    memb.Comment = "";
                    udata.ChangedUserAppMembers = new UserAppMember[] { memb };
                    var v = await usvc.AddOrUpdateEntitiesAsync(cctx, us, new User[] { udata });
                    if (v.ChangedEntities.Length == 1 && IsValidUpdate(v.ChangedEntities[0].OpStatus))
                    {
                        user.UpdateInstance(v.ChangedEntities[0].UpdatedItem);
                        return;
                    }
                    throw getException("7546b51813a82246cd88f6c9c8ff5997", "Add user failed!");
                }
                else if ((user as User).Password == lu[0].Password)
                {
                    // case of an existing user trying to join an application
                    DateTime createDate = DateTime.UtcNow;
#if MemberPlus
#else
                    udata = lu[0];
                    if (udata.Email != user.Email)
                    {
                        udata.Email = user.Email;
                        udata.IsEmailModified = true;
                        // no need to wait since it's already async on the server side.
                        usvc.EnqueueNewOrUpdateEntitiesAsync(cctx, us, new User[] { udata });
                    }
#endif
                    UserAppMemberServiceProxy membsvc = new UserAppMemberServiceProxy();
                    UserAppMember memb = await membsvc.LoadEntityByKeyAsync(cctx, app.ID, udata.ID);
                    if (memb == null)
                    {
                        memb = new UserAppMember();
                        memb.IsPersisted = false;
                        memb.ApplicationID = app.ID;
                        memb.UserID = udata.ID;
                        memb.MemberStatus = ums.MemberStatusValues[0];
                        memb.LastActivityDate = createDate;
                        var v = membsvc.AddOrUpdateEntities(cctx, ums, new UserAppMember[] { memb });
                        if (v.ChangedEntities.Length == 1 && IsValidUpdate(v.ChangedEntities[0].OpStatus))
                        {
                            user.UpdateInstance(udata);
                            return;
                        }
                        throw getException("c12be7bc5f311f434ad4df9fc6774d5c", "Add user membership failed!");
                    }
#if MemberPlus
                    else
                    {
                        if (memb.Email != user.Email)
                        {
                            if (RequiresUniqueEmail)
                            {
                                //
                                // query tokens can be merged into one
                                //
                                QueryExpresion qexpr = new QueryExpresion();
                                qexpr.OrderTks = new List<QToken>(new QToken[] { 
                                    new QToken { TkName = "UserID" },
                                    new QToken { TkName = "asc" }
                                });
                                qexpr.FilterTks = new List<QToken>(new QToken[] { 
                                    new QToken { TkName = "UserRef.Username != \"" + user.Username + "\"" },
                                    new QToken { TkName = "&&" },
                                    new QToken { TkName = "Email == \"" + user.Email + "\"" }
                                });
                                var ul = await membsvc.QueryDatabaseAsync(cctx, ums, qexpr);
                                if (ul != null && ul.Count() > 0)
                                {
                                    string err = string.Format(ResourceUtils.GetString("6e3b27059a51ececb2d27a763d1ad639", "The email address \"{0}\" is used by other user under a different user name."), user.Email);
                                    throw new NotSupportedException(err);
                                }
                            }
                            memb.Email = user.Email;
                            memb.IsEmailModified = true;
                            // no need to wait since it's already async on the server side.
                            membsvc.EnqueueNewOrUpdateEntitiesAsync(cctx, ums, new UserAppMember[] { memb });
                        }
                    }
#endif
                }
                else
                {
                    throw getException("06f02a9ab4a25c494ef92dbf3c35a16d", "User name exists!");
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "CreateUser");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
            }
        }

        public async Task DeleteAsync(TUser user)
        {
            CallContext cctx = _cctx.CreateCopy();
            UserServiceProxy usvc = new UserServiceProxy();
            try
            {
                List<User> l = await usvc.LoadEntityByNatureAsync(cctx, user.UserName);
                if (l == null || l.Count == 0)
                    return;
                User u = l[0];
                if (DeleteUserMembershipOnly)
                {
                    UserAppMemberServiceProxy msvc = new UserAppMemberServiceProxy();
                    UserAppMember memb = await msvc.LoadEntityByKeyAsync(cctx, app.ID, u.ID);
                    var result = await msvc.DeleteEntitiesAsync(cctx, new UserAppMemberSet(), new UserAppMember[] { memb });
                    // delete all profiles for the user under the current application
                    UserProfileServiceProxy upsvc = new UserProfileServiceProxy();
                    UserProfileSet ps = new UserProfileSet();
                    UserProfileSetConstraints upcond = new UserProfileSetConstraints
                    {
                        ApplicationIDWrap = new ForeignKeyData<string> { KeyValue = app.ID },
                        TypeIDWrap = null, // all types of the profiles will be included.
                        UserIDWrap = new ForeignKeyData<string> { KeyValue = u.ID }
                    };
                    var pl = await upsvc.ConstraintQueryAsync(cctx, ps, upcond, null);
                    if (pl.Count() > 0)
                    {
                        await upsvc.DeleteEntitiesAsync(cctx, ps, pl.ToArray());
                    }
                    // delete all role assignments associated with the user
                    UsersInRoleServiceProxy uisvc = new UsersInRoleServiceProxy();
                    UsersInRoleSetConstraints uircond = new UsersInRoleSetConstraints
                    {
                        RoleIDWrap = null,
                        UserIDWrap = new ForeignKeyData<string> { KeyValue = u.ID }
                    };
                    var lir = await uisvc.ConstraintQueryAsync(cctx, new UsersInRoleSet(), uircond, null);
                    if (lir.Count() > 0)
                    {
                        await uisvc.DeleteEntitiesAsync(cctx, new UsersInRoleSet(), lir.ToArray());
                    }
                }
                else
                {
                    await usvc.DeleteEntitiesAsync(cctx, new UserSet(), new User[] { u });
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteUser");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
            }
        }

        public async Task<TUser> FindByIdAsync(string userId)
        {
            CallContext cctx = _cctx.CreateCopy();
            UserServiceProxy usvc = new UserServiceProxy();
            var u = await usvc.LoadEntityByKeyAsync(cctx, userId);
            if (u == null)
                return null;
            var user = new TUser();
            user.UpdateInstance(u);
            UserAppMemberSet membs = new UserAppMemberSet();
            UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
            var memb = await mbsvc.LoadEntityByKeyAsync(cctx, app.ID, user.Id);
            if (memb != null)
            {
                user.AppMemberStatus = memb.MemberStatus;
                if (memb.MemberStatus == membs.MemberStatusValues[0])
                {
                    var roles = await GetRolesAsync(user);
                    foreach (var r in roles)
                        user.Claims.Add(CreateClaim(Microsoft.IdentityModel.Claims.ClaimTypes.Role, r));
                }
#if MemberPlus
                user.Email = memb.Email;
                if (!string.IsNullOrEmpty(user.Email))
                    user.Claims.Add(UserStore<ApplicationUser>.CreateClaim(Microsoft.IdentityModel.Claims.ClaimTypes.Email, user.Email));
                user.HasIcon = (!string.IsNullOrEmpty(memb.IconMime));
                if (user.HasIcon)
                    user.Claims.Add(UserStore<ApplicationUser>.CreateClaim(CustomClaims.HasIcon, true.ToString()));
#endif
            }
            return user;
        }

        public async Task<TUser> FindByNameAsync(string userName)
        {
            CallContext cctx = _cctx.CreateCopy();
            UserServiceProxy usvc = new UserServiceProxy();
            var us = await usvc.LoadEntityByNatureAsync(cctx, userName);
            if (us == null || us.Count == 0)
                return null;
            var user = new TUser();
            user.UpdateInstance(us[0]);
            UserAppMemberSet membs = new UserAppMemberSet();
            UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
            var memb = await mbsvc.LoadEntityByKeyAsync(cctx, app.ID, user.Id);
            if (memb != null)
            {
                user.AppMemberStatus = memb.MemberStatus;
                if (memb.MemberStatus == membs.MemberStatusValues[0])
                {
                    var roles = await GetRolesAsync(user);
                    foreach (var r in roles)
                        user.Claims.Add(CreateClaim(Microsoft.IdentityModel.Claims.ClaimTypes.Role, r));
                }
#if MemberPlus
                user.Email = memb.Email;
                if (!string.IsNullOrEmpty(user.Email))
                    user.Claims.Add(UserStore<ApplicationUser>.CreateClaim(Microsoft.IdentityModel.Claims.ClaimTypes.Email, user.Email));
                user.HasIcon = (!string.IsNullOrEmpty(memb.IconMime));
                if (user.HasIcon)
                    user.Claims.Add(UserStore<ApplicationUser>.CreateClaim(CustomClaims.HasIcon, true.ToString()));
#endif
            }
            return user;
        }

        public async Task<string> GetUserNameByEmailAsync(string email)
        {
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { new QToken { TkName = "Username" } });
#if MemberPlus
            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                    new QToken { TkName = "UserAppMember." },
                    new QToken { TkName = "Email" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "\"" + email + "\"" }
                });
#else
            qexpr.FilterTks = new List<QToken>(new QToken[]{
                    new QToken { TkName = "Email" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "\"" + email + "\"" }
                });
#endif
            CallContext cctx = _cctx.CreateCopy();
            UserServiceProxy usvc = new UserServiceProxy();
            var l = await usvc.QueryDatabaseAsync(cctx, new UserSet(), qexpr);
            if (l == null || l.Count() == 0)
                return null;
            return l.First().Username;
        }

        public async Task<string> GetPasswordHashAsync(TUser user)
        {
            if ((user as User).IsPersisted)
            {
                CallContext cctx = _cctx.CreateCopy();
                UserServiceProxy usvc = new UserServiceProxy();
                var u = await usvc.LoadEntityByKeyAsync(cctx, user.Id);
                if (u != null)
                    (user as User).Password = u.Password;
                return u != null ? u.Password : null;
            }
            else if (user != null)
                return (user as User).Password;
            else
                return null;
        }

        public async Task<bool> HasPasswordAsync(TUser user)
        {
            return true;
        }

        public async Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            User u = user as User;
            u.Password = passwordHash;
            if (u.IsPersisted)
            {
                CallContext cctx = _cctx.CreateCopy();
                UserServiceProxy usvc = new UserServiceProxy();
                u = await usvc.LoadEntityByKeyAsync(cctx, user.Id);
                if (u.Password != passwordHash)
                {
                    u.Password = passwordHash;
                    u.IsPasswordModified = true;
                    await usvc.AddOrUpdateEntitiesAsync(cctx, new UserSet(), new User[] { u });
                }
            }
        }

        public async Task UpdateFailureCountAsync(CallContext cctx, TUser user, string failureType)
        {
            bool b = cctx.DirectDataAccess;
            cctx.DirectDataAccess = true;
            UserServiceProxy usvc = new UserServiceProxy();
            UserAppMemberServiceProxy umsvc = new UserAppMemberServiceProxy();
            try
            {
                User u = new User();
                u.IsPersisted = false;
                User.MergeChanges(user as User, u);
                u.IsPersisted = user.IsPersisted;
                DateTime windowStart = new DateTime();
                int failureCount = 0;
                if (failureType == "password")
                {
                    failureCount = u.FailedPasswordAttemptCount.HasValue ? u.FailedPasswordAttemptCount.Value : 0;
                    windowStart = u.FailedPasswordAttemptWindowStart.HasValue ? u.FailedPasswordAttemptWindowStart.Value : DateTime.MinValue;
                }
                else if (failureType == "passwordAnswer")
                {
                    failureCount = u.FailedPasswordAnswerAttemptCount.HasValue ? u.FailedPasswordAnswerAttemptCount.Value : 0;
                    windowStart = u.FailedPasswordAnswerAttemptWindowStart.HasValue ? u.FailedPasswordAnswerAttemptWindowStart.Value : DateTime.MinValue;
                }
                DateTime windowEnd = windowStart.AddSeconds(PasswordAttemptWindow);
                //repo.BeginRepoTransaction(cctx);
                if (failureCount == 0 || DateTime.UtcNow > windowEnd)
                {
                    if (failureType == "password")
                    {
                        u.FailedPasswordAttemptCount = 1;
                        u.IsFailedPasswordAttemptCountModified = true;
                        u.FailedPasswordAttemptWindowStart = DateTime.UtcNow;
                        u.IsFailedPasswordAttemptWindowStartModified = true;
                    }
                    else if (failureType == "passwordAnswer")
                    {
                        u.FailedPasswordAnswerAttemptCount = 1;
                        u.IsFailedPasswordAnswerAttemptCountModified = true;
                        u.FailedPasswordAnswerAttemptWindowStart = DateTime.UtcNow;
                        u.IsFailedPasswordAnswerAttemptWindowStartModified = true;
                    }
                    await usvc.AddOrUpdateEntitiesAsync(cctx, new UserSet(), new User[] { u as User });
                }
                else
                {
                    if (++failureCount >= MaxInvalidPasswordAttempts)
                    {
                        UserAppMemberSet us = new UserAppMemberSet();
                        UserAppMember um = await umsvc.LoadEntityByKeyAsync(cctx, app.ID, u.ID);
                        if (um != null)
                        {
                            um.MemberStatus = us.MemberStatusValues[3];
                            um.IsMemberStatusModified = true;
                            um.LastStatusChange = DateTime.UtcNow;
                            um.IsLastStatusChangeModified = true;
                            await umsvc.AddOrUpdateEntitiesAsync(cctx, us, new UserAppMember[] { um });
                        }
                    }
                    else
                    {
                        if (failureType == "password")
                        {
                            u.FailedPasswordAttemptCount = failureCount;
                            u.IsFailedPasswordAttemptCountModified = true;
                            u.FailedPasswordAttemptWindowStart = DateTime.UtcNow;
                            u.IsFailedPasswordAttemptWindowStartModified = true;
                        }
                        else if (failureType == "passwordAnswer")
                        {
                            u.FailedPasswordAnswerAttemptCount = failureCount;
                            u.IsFailedPasswordAnswerAttemptCountModified = true;
                            u.FailedPasswordAnswerAttemptWindowStart = DateTime.UtcNow;
                            u.IsFailedPasswordAnswerAttemptWindowStartModified = true;
                        }
                        await usvc.AddOrUpdateEntitiesAsync(cctx, new UserSet(), new User[] { u as User });
                    }
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateFailureCount");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
                cctx.DirectDataAccess = b;
            }
        }

        public async Task AddToRoleAsync(TUser user, string role)
        {
            CallContext cctx = _cctx.CreateCopy();
            RoleServiceProxy rsvc = new RoleServiceProxy();
            try
            {
                UserServiceProxy usvc = new UserServiceProxy();
                UsersInRoleServiceProxy ursvc = new UsersInRoleServiceProxy();
                List<User> ul = await usvc.LoadEntityByNatureAsync(cctx, user.UserName);
                if (ul == null || ul.Count == 0)
                {
#if MemberPlus
                    throw new ArgumentException(string.Format(ResourceUtils.GetString("b66098049404e4de1356242e8aa6444a", "User \"{0}\" is not found."), user.UserName));
#else
                    throw new ArgumentException("User '" + user.UserName + "' is not found.");
#endif
                }
                User u = ul[0];
                user.UpdateInstance(u);
                Role r = await findRoleAsync(role);
                if (UserStoreAutoCleanupRoles)
                {
                    var curr_roles = await _getRolesAsync(user);
                    // try to find more specific roles
                    bool alreadyin = false;
                    foreach (var kvp in curr_roles)
                    {
                        var curr_r = kvp.Key;
                        while (curr_r.ParentID != null)
                        {
                            if (curr_r.UpperRef.ID == r.ID)
                            {
                                alreadyin = true;
                                break;
                            }
                            curr_r = curr_r.UpperRef;
                        }
                        if (alreadyin)
                            break;
                    }
                    if (alreadyin)
                        return;
                    List<UsersInRole> deleted = new List<UsersInRole>();
                    foreach (var kvp in curr_roles)
                    {
                        //delete all roles that are less specific than the current one.
                        if (role.StartsWith(kvp.Value + "."))
                            deleted.Add(new UsersInRole { RoleID = kvp.Key.ID, UserID = user.ID });
                    }
                    if (deleted.Count > 0)
                    {
                        ursvc.DeleteEntities(cctx, new UsersInRoleSet(), deleted.ToArray());
                    }
                }
                List<UsersInRole> l = new List<UsersInRole>();
                UsersInRole uir = new UsersInRole();
                uir.RoleID = r.ID;
                uir.UserID = u.ID;
                l.Add(uir);
                var result = await ursvc.AddOrUpdateEntitiesAsync(cctx, new UsersInRoleSet(), l.ToArray());
                if (result.ChangedEntities.Length != 1 || !IsValidUpdate(result.ChangedEntities[0].OpStatus))
                {
#if MemberPlus
                    throw new Exception(string.Format(ResourceUtils.GetString("990248539c7ca97e045c440fbec05bf3", "Add role: \"{0}\" to user: \"{1}\" failed."), role, user.Username));
#else
                    throw new Exception("Add role: \"" + role + "\" to user: " + user.UserName + " failed.");
#endif
                }
                // cleanup, delete more general roles to make them implicit
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "AddUsersToRoles");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
            }
        }

        public async Task<System.Collections.Generic.IList<string>> GetRolesAsync(TUser user)
        {
            var dic = await _getRolesAsync(user);
            return (from d in dic select d.Value).ToList();
        }

        private async Task<Dictionary<Role, string>> _getRolesAsync(TUser user)
        {
            CallContext cctx = _cctx.CreateCopy();
            Dictionary<Role, string> list = new Dictionary<Role, string>();
            try
            {
                UserServiceProxy usvc = new UserServiceProxy();
                var lu = await usvc.LoadEntityByNatureAsync(cctx, user.UserName);
                if (lu == null || lu.Count == 0)
                    return list;
                User u = lu[0];
                RoleServiceProxy rsvc = new RoleServiceProxy();
                QueryExpresion qexpr = new QueryExpresion();
                qexpr.OrderTks = new List<QToken>(new QToken[] { new QToken { TkName = "RoleName" } });
                qexpr.FilterTks = new List<QToken>(new QToken[]{
                    new QToken { TkName = "ApplicationID" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "\"" + app.ID + "\"" },
                    new QToken { TkName = "&&" },
                    new QToken { TkName = "UsersInRole." },
                    new QToken { TkName = "UserID" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "\"" + u.ID + "\"" }
                });
                var roles = await rsvc.QueryDatabaseAsync(cctx, new RoleSet(), qexpr);
                foreach (Role r in roles)
                {
                    //
                    // if a user is in a role, then he/she is in the parent roles (if any) of that role as well, this rule is also applied to the parent role ....
                    //
                    if (r.ParentID != null)
                    {
                        Stack<Role> srs = new Stack<Role>();
                        Role pr = r;
                        while (pr != null)
                        {
                            srs.Push(pr);
                            var p = await rsvc.MaterializeUpperRefAsync(cctx, pr);
                            pr.UpperRef = p;
                            pr = p;
                        }
                        while (srs.Count > 0)
                        {
                            Role _r = srs.Pop();
                            string rp = await rolePathAsync(_r);
                            if (!(from d in list where d.Value == rp select d).Any())
                                list.Add(_r, rp);
                        }
                    }
                    else
                    {
                        string rp = await rolePathAsync(r);
                        if (!(from d in list where d.Value == rp select d).Any())
                            list.Add(r, rp);
                    }
                }
                return list;
            }
            finally
            {
            }
        }

        public async Task<bool> IsInRoleAsync(TUser user, string role)
        {
            CallContext cctx = _cctx.CreateCopy();
            try
            {
                UserServiceProxy usvc = new UserServiceProxy();
                var lu = await usvc.LoadEntityByNatureAsync(cctx, user.UserName);
                if (lu == null || lu.Count == 0)
                    return false;
                User u = lu[0];
                Role r = await findRoleAsync(role);
                if (r == null)
                    return false;
                UsersInRoleServiceProxy uisvc = new UsersInRoleServiceProxy();
                UsersInRole x = await uisvc.LoadEntityByKeyAsync(cctx, r.ID, u.ID);
                if (x != null)
                    return true;
                else
                {
                    RoleServiceProxy rsvc = new RoleServiceProxy();
                    var ra = await rsvc.LoadEntityHierarchyRecursAsync(cctx, r, 0, -1);
                    //for a given role, the users in it also include the ones in all its child roles, recursively (see above), in addition to its own ...
                    List<string> uns = new List<string>();
                    await _getUserInRoleAsync(cctx, ra, uns);
                    return (from d in uns where d == user.UserName select d).Any();
                }
            }
            finally
            {
            }
        }

        private async Task _getUserInRoleAsync(CallContext cctx, EntityAbs<Role> ra, List<string> usersinrole)
        {
            UserServiceProxy usvc = new UserServiceProxy();
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { new QToken { TkName = "Username" } });
            qexpr.FilterTks = new List<QToken>(new QToken[]{
                    new QToken { TkName = "UsersInRole." },
                    new QToken { TkName = "RoleID" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "" + ra.DataBehind.ID + "" }
                });
            var users = await usvc.QueryDatabaseAsync(cctx, new UserSet(), qexpr);
            foreach (User u in users)
                usersinrole.Add(u.Username);
            if (ra.ChildEntities != null)
            {
                foreach (var c in ra.ChildEntities)
                    await _getUserInRoleAsync(cctx, c, usersinrole);
            }
        }

        public async Task RemoveFromRoleAsync(TUser user, string role)
        {
            CallContext cctx = _cctx.CreateCopy();
            try
            {
                UserServiceProxy usvc = new UserServiceProxy();
                var lu = await usvc.LoadEntityByNatureAsync(cctx, user.UserName);
                if (lu == null || lu.Count == 0)
                    return;
                User u = lu[0];
                Role r = await findRoleAsync(role);
                if (r != null)
                {
                    UsersInRoleServiceProxy uisvc = new UsersInRoleServiceProxy();
                    UsersInRole uir = await uisvc.LoadEntityByKeyAsync(cctx, r.ID, u.ID);
                    if (uir != null)
                        await uisvc.DeleteEntitiesAsync(cctx, new UsersInRoleSet(), new UsersInRole[] { uir });
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "RemoveUsersFromRoles");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
            }
        }

        public async Task UpdateAsync(TUser user)
        {
            UserServiceProxy usvc = new UserServiceProxy();
            CallContext cctx = _cctx.CreateCopy();
            try
            {
                User u = await usvc.LoadEntityByKeyAsync(cctx, user.Id);
                if (u != null)
                {
                    UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
                    UserAppMemberSetConstraints cond = new UserAppMemberSetConstraints
                    {
                        ApplicationIDWrap = new ForeignKeyData<string> { KeyValue = app.ID },
                        UserIDWrap = new ForeignKeyData<string> { KeyValue = u.ID }
                    };
                    var mbset = new UserAppMemberSet();
                    var mbs = await mbsvc.ConstraintQueryAsync(cctx, mbset, cond, null);
                    UserAppMember memb = mbs.FirstOrDefault();
                    int ccnt = 0;
                    int mccnt = 0;
#if MemberPlus
                    if (memb.Email != user.Email)
                    {
                        if (RequiresUniqueEmail)
                        {
                            //
                            // query tokens can be merged into one
                            //
                            QueryExpresion qexpr = new QueryExpresion();
                            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                                new QToken { TkName = "UserID" },
                                new QToken { TkName = "asc" }
                            });
                            qexpr.FilterTks = new List<QToken>(new QToken[] { 
                                new QToken { TkName = "UserRef.Username != \"" + user.Username + "\"" },
                                new QToken { TkName = "&&" },
                                new QToken { TkName = "Email == \"" + user.Email + "\"" }
                            });
                            var ul = await mbsvc.QueryDatabaseAsync(cctx, mbset, qexpr);
                            if (ul != null && ul.Count() > 0)
                            {
                                string err = string.Format(ResourceUtils.GetString("6e3b27059a51ececb2d27a763d1ad639", "The email address \"{0}\" is used by other user under a different user name."), user.Email);
                                throw new NotSupportedException(err);
                            }
                        }
                        memb.Email = user.Email;
                        memb.IsEmailModified = true;
                        mccnt++;
                    }
#else
                    if (u.Email != user.Email)
                    {
                        if (RequiresUniqueEmail)
                        {
                            //
                            // query tokens can be merged into one
                            //
                            QueryExpresion qexpr = new QueryExpresion();
                            qexpr.OrderTks = new List<QToken>(new QToken[] { new QToken { TkName = "Username" } });
                            qexpr.FilterTks = new List<QToken>(new QToken[]
                            {
                                new QToken { TkName = "Email == \"" + user.Email + "\"" }
                            });
                            var ul = await usvc.QueryDatabaseAsync(cctx, new UserSet(), qexpr);
                            if (ul != null && ul.Count() > 0)
                            {
                                throw new NotSupportedException("The email address \"" + user.Email + "\" exists inside the data source.");
                            }
                        }
                        u.Email = user.Email;
                        u.IsEmailModified = true;
                        ccnt++;
                    }
#endif
                    if ((user as User).Password != u.Password)
                    {
                        u.Password = (user as User).Password;
                        u.IsPasswordModified = true;
                        ccnt++;
                    }
                    if (memb.MemberStatus != user.AppMemberStatus)
                    {
                        memb.MemberStatus = user.AppMemberStatus;
                        memb.IsMemberStatusModified = true;
                        memb.LastStatusChange = DateTime.UtcNow;
                        memb.IsLastStatusChangeModified = true;
                        mccnt++;
                    }
                    /*
                    if (!memb.IsCommentLoaded)
                    {
                        memb.Comment = mbsvc.LoadEntityComment(cctx, app.ID, u.ID);
                        memb.IsCommentLoaded = true;
                    }
                    if (memb.Comment != user.Comment)
                    {
                        memb.Comment = user.Comment;
                        memb.IsCommentModified = true;
                        mccnt++;
                    }
                    */
                    if (u.IsApproved != (user as User).IsApproved)
                    {
                        u.IsApproved = (user as User).IsApproved;
                        u.IsIsApprovedModified = true;
                        ccnt++;
                    }
                    if (u.PasswordQuestion != user.PasswordQuestion)
                    {
                        u.PasswordQuestion = user.PasswordQuestion;
                        u.IsPasswordQuestionModified = true;
                        ccnt++;
                    }
                    if (u.PasswordAnswer != user.PasswordAnswer)
                    {
                        u.PasswordAnswer = user.PasswordAnswer;
                        u.IsPasswordAnswerModified = true;
                        ccnt++;
                    }
                    if (ccnt > 0)
                    {
                        u.IsEntityChanged = true;
                        await usvc.AddOrUpdateEntitiesAsync(cctx, new UserSet(), new User[] { u });
                    }
                    if (mccnt > 0)
                    {
                        memb.IsEntityChanged = true;
                        await mbsvc.AddOrUpdateEntitiesAsync(cctx, new UserAppMemberSet(), new UserAppMember[] { memb });
                    }
                }
            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateUser");
                }
                throw getException("cb5e100e5a9a3e7f6d1fd97512215283", "error", e);
            }
            finally
            {
            }
        }

        public virtual Task AddClaimAsync(TUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public virtual Task<System.Collections.Generic.IList<Claim>> GetClaimsAsync(TUser user)
        {
            throw new NotImplementedException();
        }

        public virtual Task RemoveClaimAsync(TUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public static Claim CreateClaim(string type, string value)
        {
            return new Claim(type, value, "string", NameProviderId);
        }

        public static Claim CreateClaim(string type, string value, string valueType)
        {
            return new Claim(type, value, valueType, NameProviderId);
        }

        //public Task AddLoginAsync(TUser user, UserLoginInfo login)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<TUser> FindAsync(UserLoginInfo login)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<System.Collections.Generic.IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        //{
        //    return null;
        //}

        //public Task<string> GetSecurityStampAsync(TUser user)
        //{
        //    return null;
        //}


        //public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        //{
        //    return null;
        //}

        //public Task SetSecurityStampAsync(TUser user, string stamp)
        //{
        //    return null;
        //}

        public async Task<IdentityResult> ValidateAsync(string item)
        {
            //temp ...
            return IdentityResult.Success;
        }

        public string HashPassword(string password)
        {
            return EncodePassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return hashedPassword == EncodePassword(providedPassword) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }

        private async Task<Role> findRoleAsync(string rolename)
        {
            var x = await _findRoleAsync(rolename);
            return x == null ? null : x.Item1;
        }

        private async Task<Tuple<Role, Role>> _findRoleAsync(string rolename)
        {
            if (string.IsNullOrEmpty(rolename))
                return null;
            CallContext cctx = _cctx.CreateCopy();
            string[] rolepath = rolename.Trim('.').Split('.');
            RoleServiceProxy rsvc = new RoleServiceProxy();
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { new QToken { TkName = "RoleName" } });
            qexpr.FilterTks = new List<QToken>(new QToken[]{
                    new QToken { TkName = "ApplicationID" },
                    new QToken { TkName = "==" },
                    new QToken { TkName = "\"" + app.ID + "\"" },
                    new QToken { TkName = "&&" },
                    new QToken { TkName = "ParentID" },
                    new QToken { TkName = "is null" }
                });
            var rrts = await rsvc.QueryDatabaseAsync(cctx, new RoleSet(), qexpr);
            Role last = null;
            foreach (var rr in rrts)
            {
                if (rr.RoleName == rolepath[0])
                {
                    if (rolepath.Length > 1)
                    {
                        var rtree = await rsvc.LoadEntityFullHierarchyRecursAsync(cctx, rr);
                        last = rtree.DataBehind;
                        var r = findMatch(rtree, rolepath, 1, ref last);
                        return new Tuple<Role, Role>(r, last);
                    }
                    else
                    {
                        return new Tuple<Role, Role>(rr, rr);
                    }
                }
            }
            return null;
        }

        private Role findMatch(EntityAbs<Role> ra, string[] path, int lev, ref Role last)
        {
            if (ra.ChildEntities != null)
            {
                foreach (var c in ra.ChildEntities)
                {
                    if (c.DataBehind.RoleName == path[lev])
                    {
                        c.DataBehind.UpperRef = last;
                        last = c.DataBehind;
                        if (lev == path.Length - 1)
                            return c.DataBehind;
                        else
                            return findMatch(c, path, lev + 1, ref last);
                    }
                }
            }
            return null;
        }

        private async Task<string> rolePathAsync(Role r)
        {
            RoleServiceProxy rsvc = null;
            string rpath = r.RoleName;
            while (r.ParentID != null)
            {
                if (r.UpperRef == null)
                {
                    if (rsvc == null)
                        rsvc = new RoleServiceProxy();
                    r.UpperRef = await rsvc.MaterializeUpperRefAsync(_cctx.CreateCopy(), r);
                }
                rpath = r.UpperRef.RoleName + "." + rpath;
                r = r.UpperRef;
            }
            return rpath;
        }

        private bool IsValidUpdate(int status)
        {
            return (status & (int)EntityOpStatus.Added) > 0 || (status & (int)EntityOpStatus.Updated) > 0 || (status & (int)EntityOpStatus.NoOperation) > 0;
        }

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            HMACSHA256 hash = new HMACSHA256();
#if TEST
            hash.Key = HexToByte("FDD492BE84F54EEEA666AB092EC37CE37FAEED953BB905DDBD0ABDAFAAEB3972A90271EBD00104CC3E41A183F61D680EC2697C717EF3FC00AA67451DA6DF9BC1");
#else
            hash.Key = HexToByte(machineKey.ValidationKey);
#endif
            encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));

            return encodedPassword;
        }

        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        private void WriteToEventLog(Exception e, string action)
        {
            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action;
            Trace.Write(message);
            Debug.Write(message);
            /*
            if (log.IsErrorEnabled)
                log.Error("[" + cctx.InVokePath + "]: " + message, e);
            */
        }

        public void Dispose()
        {

        }

        protected virtual void Dispose(bool disposing)
        {

        }

    }
}
