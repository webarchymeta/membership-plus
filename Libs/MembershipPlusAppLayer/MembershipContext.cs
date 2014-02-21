using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer.Models;
using Archymeta.Web.Security;
using Archymeta.Web.Security.Resources;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class MembershipContext
    {
        internal static CallContext Cntx
        {
            get
            {
                return ApplicationContext.ClientContext.CreateCopy();
            }
        }

        public static async Task<ContentRecord> GetMemberIcon(string id)
        {
            UserAppMemberServiceProxy umsvc = new UserAppMemberServiceProxy();
            var um = await umsvc.LoadEntityByKeyAsync(Cntx, ApplicationContext.App.ID, id);
            if (um == null)
                return null;
            ContentRecord rec = new ContentRecord();
            rec.MimeType = um.IconMime;
            rec.LastModified = um.IconLastModified.HasValue ? um.IconLastModified.Value : DateTime.MaxValue;
            rec.Data = await umsvc.LoadEntityIconImgAsync(Cntx, ApplicationContext.App.ID, id);
            return rec;
        }

        public static async Task<bool> UpdateMemeberIcon(string id, string mineType, DateTime lastModified, byte[] imgdata)
        {
            UserAppMemberServiceProxy umsvc = new UserAppMemberServiceProxy();
            var um = umsvc.LoadEntityByKey(Cntx, ApplicationContext.App.ID, id);
            if (um == null)
                return false;
            um.IconImg = imgdata;
            um.IsIconImgLoaded = true;
            um.IsIconImgModified = true;
            um.IconMime = mineType;
            um.IconLastModified = lastModified;
            var result = await umsvc.AddOrUpdateEntitiesAsync(Cntx, new UserAppMemberSet(), new UserAppMember[] { um });
            return (result.ChangedEntities[0].OpStatus & (int)EntityOpStatus.Updated) > 0;
        }

        public static async Task ChangeAccountInfo(string id, ApplicationUser user)
        {
            var cntx = Cntx;
            UserServiceProxy usvc = new UserServiceProxy();
            var u = await usvc.LoadEntityByKeyAsync(cntx, id);
            if (u == null)
                return;
            u.FirstName = user.FirstName;
            u.LastName = user.LastName;
            if (u.IsFirstNameModified || u.IsLastNameModified)
                await usvc.AddOrUpdateEntitiesAsync(cntx, new UserSet(), new User[] { u });
            if (!string.IsNullOrEmpty(user.Email))
            {
                UserAppMemberServiceProxy mbsvc = new UserAppMemberServiceProxy();
                var mb = await mbsvc.LoadEntityByKeyAsync(cntx, ApplicationContext.App.ID, id);
                if (mb != null)
                {
                    mb.Email = user.Email;
                    if (mb.IsEmailModified)
                        await mbsvc.AddOrUpdateEntitiesAsync(cntx, new UserAppMemberSet(), new UserAppMember[] { mb });
                }
            }
        }

        public static async Task<UserDetailsVM> GetUserDetails(string id, bool direct = false)
        {
            UserDetailServiceProxy udsvc = new UserDetailServiceProxy();
            var cntx = Cntx;
            cntx.DirectDataAccess = direct;
            var details = await udsvc.LoadEntityByKeyAsync(cntx, ApplicationContext.App.ID, id);
            UserDetailsVM m = null;
            if (details != null)
            {
                if (!details.IsDescriptionLoaded)
                {
                    details.Description = udsvc.LoadEntityDescription(cntx, ApplicationContext.App.ID, id);
                    details.IsDescriptionLoaded = true;
                }
                m = new UserDetailsVM { Details = details };
                m.IsPhotoAvailable = !string.IsNullOrEmpty(details.PhotoMime);
            }
            else
            {
                m = new UserDetailsVM();
                m.IsPhotoAvailable = false;
            }
            UserDetailSet uds = new UserDetailSet();
            m.Genders = uds.GenderValues;
            QueryExpresion qexpr = new QueryExpresion();
            qexpr.OrderTks = new List<QToken>(new QToken[] { 
                new QToken { TkName = "ID" },
                new QToken { TkName = "asc" }
            });
            CommunicationTypeServiceProxy ctsvc = new CommunicationTypeServiceProxy();
            var lct = await ctsvc.QueryDatabaseAsync(Cntx, new CommunicationTypeSet(), qexpr);
            m.ChannelTypes = lct.ToArray();
            CommunicationServiceProxy csvc = new CommunicationServiceProxy();
            //qexpr = new QueryExpresion();
            //qexpr.OrderTks = new List<QToken>(new QToken[] { 
            //    new QToken { TkName = "TypeID" },
            //    new QToken { TkName = "asc" }
            //});
            //qexpr.FilterTks = new List<QToken>(new QToken[] { 
            //    new QToken { TkName = "UserID" },
            //    new QToken { TkName = "==" },
            //    new QToken { TkName = "\"" + id + "\"" },
            //    new QToken { TkName = "&&" },
            //    new QToken { TkName = "ApplicationID" },
            //    new QToken { TkName = "==" },
            //    new QToken { TkName = "\"" + ApplicationContext.App.ID + "\"" }
            //});
            //var lc = await csvc.QueryDatabaseAsync(Cntx, new CommunicationSet(), qexpr);
            var fkeys = new CommunicationSetConstraints
            {
                ApplicationIDWrap = new ForeignKeyData<string> { KeyValue = ApplicationContext.App.ID },
                TypeIDWrap = null, // no restriction on types
                UserIDWrap = new ForeignKeyData<string> { KeyValue = id }
            };
            var lc = await csvc.ConstraintQueryAsync(Cntx, new CommunicationSet(), fkeys, null);
            foreach (var c in lc)
            {
                c.Comment = await csvc.LoadEntityCommentAsync(Cntx, c.ID);
                c.IsCommentLoaded = true;
                c.CommunicationTypeRef = await csvc.MaterializeCommunicationTypeRefAsync(Cntx, c);
                m.Channels.Add(new { id = c.ID, label = c.DistinctString, addr = c.AddressInfo, comment = c.Comment, typeId = c.CommunicationTypeRef.TypeName });
            }
            return m;
        }

        public static async Task<bool> CreateUserDetails(string id)
        {
            UserDetailServiceProxy udsvc = new UserDetailServiceProxy();
            var ud = await udsvc.LoadEntityByKeyAsync(Cntx, ApplicationContext.App.ID, id);
            if (ud == null)
            {
                await udsvc.AddOrUpdateEntitiesAsync(Cntx, new UserDetailSet(), new UserDetail[] { 
                    new UserDetail{
                        UserID = id,
                        ApplicationID = ApplicationContext.App.ID,
                        CreateDate = DateTime.UtcNow
                    }
                });
            }
            return true;
        }

        public static async Task<UserDetailsVM> UpdateUserProperties(string id, UserDetailsVM model)
        {
            UserDetailServiceProxy udsvc = new UserDetailServiceProxy();
            var cntx = Cntx;
            var details = await udsvc.LoadEntityByKeyAsync(cntx, ApplicationContext.App.ID, id);
            int chgcnt = 0;
            if (details.Gender != model.Gender)
            {
                details.Gender = model.Gender;
                chgcnt++;
            }
            if (!details.BirthDate.HasValue && model.BirthDate.HasValue || details.BirthDate.HasValue && !model.BirthDate.HasValue ||
                details.BirthDate.HasValue && model.BirthDate.HasValue && details.BirthDate.Value != model.BirthDate.Value)
            {
                details.BirthDate = model.BirthDate;
                chgcnt++;
            }
            if (chgcnt > 0)
            {
                details.LastModified = DateTime.UtcNow;
                udsvc.AddOrUpdateEntities(Cntx, new UserDetailSet(), new UserDetail[] { details });
            }
            return await GetUserDetails(id, true);
        }

        public static async Task<UserDetailsVM> UpdateUserDescription(string id, UserDetailsVM model)
        {
            UserDetailServiceProxy udsvc = new UserDetailServiceProxy();
            var cntx = Cntx;
            var details = await udsvc.LoadEntityByKeyAsync(cntx, ApplicationContext.App.ID, id);
            if (details != null && !details.IsDescriptionLoaded)
            {
                details.Description = await udsvc.LoadEntityDescriptionAsync(cntx, ApplicationContext.App.ID, id);
                details.IsDescriptionLoaded = true;
            }
            bool changed = details.Description == null && model.Description != null || details.Description != null && model.Description == null ||
                details.Description != null && model.Description != null && details.Description.Trim() != model.Description.Trim();
            if (changed)
            {
                details.Description = model.Description;
                details.LastModified = DateTime.UtcNow;
                udsvc.AddOrUpdateEntities(Cntx, new UserDetailSet(), new UserDetail[] { details });
            }
            return await GetUserDetails(id, true);
        }

        public static async Task<bool> UpdateUserPhoto(string id, string mimeType, DateTime lastModified, byte[] imagedata)
        {
            UserDetailServiceProxy udsvc = new UserDetailServiceProxy();
            var ud = await udsvc.LoadEntityByKeyAsync(Cntx, ApplicationContext.App.ID, id);
            ud.PhotoMime = mimeType;
            ud.PhotoLastModified = lastModified;
            ud.Photo = imagedata;
            ud.IsPhotoLoaded = true;
            ud.IsPhotoModified = true;
            ud.LastModified = DateTime.UtcNow;
            await udsvc.AddOrUpdateEntitiesAsync(Cntx, new UserDetailSet(), new UserDetail[] { ud });
            return true;
        }

        public static async Task<ContentRecord> GetUserPhoto(string id)
        {
            UserDetailServiceProxy udsvc = new UserDetailServiceProxy();
            var ud = await udsvc.LoadEntityByKeyAsync(Cntx, ApplicationContext.App.ID, id);
            if (ud == null)
                return null;
            ContentRecord rec = new ContentRecord();
            rec.MimeType = ud.PhotoMime;
            if (ud.LastModified.HasValue)
                rec.LastModified = ud.LastModified.Value;
            rec.Data = udsvc.LoadEntityPhoto(Cntx, ApplicationContext.App.ID, id);
            return rec;
        }

        public static async Task<dynamic> AddChannel(string id, int typeId, string address, string comment)
        {
            CommunicationServiceProxy csvc = new CommunicationServiceProxy();
            Communication c = new Communication();
            c.ID = Guid.NewGuid().ToString();
            c.TypeID = typeId;
            c.UserID = id;
            c.ApplicationID = ApplicationContext.App.ID;
            c.AddressInfo = address;
            c.Comment = comment;
            var result = await csvc.AddOrUpdateEntitiesAsync(Cntx, new CommunicationSet(), new Communication[] { c });
            if ((result.ChangedEntities[0].OpStatus & (int)EntityOpStatus.Added) > 0)
            {
                c = result.ChangedEntities[0].UpdatedItem;
                c.CommunicationTypeRef = await csvc.MaterializeCommunicationTypeRefAsync(Cntx, c);
                var dc = new { id = c.ID, label = c.DistinctString, addr = c.AddressInfo, comment = c.Comment, typeId = c.CommunicationTypeRef.TypeName };
                return new { ok = true, msg = "", data = dc };
            }
            else
                return new { ok = false, msg = ResourceUtils.GetString("954122aa46fdc842a03ed8b89acdd125", "Add failed!") };
        }

        public static async Task<dynamic> UpdateChannel(string id, string address, string comment)
        {
            CommunicationServiceProxy csvc = new CommunicationServiceProxy();
            Communication c = await csvc.LoadEntityByKeyAsync(Cntx, id);
            if (c == null)
                return new { ok = false, msg = ResourceUtils.GetString("2d092e365fe439f4e11223a6aac685df", "Channel not found!") };
            c.Comment = await csvc.LoadEntityCommentAsync(Cntx, c.ID);
            c.IsCommentLoaded = true;
            c.AddressInfo = address;
            c.Comment = comment;
            if (c.IsAddressInfoModified || c.IsCommentModified)
            {
                var result = await csvc.AddOrUpdateEntitiesAsync(Cntx, new CommunicationSet(), new Communication[] { c });
                if ((result.ChangedEntities[0].OpStatus & (int)EntityOpStatus.Updated) > 0)
                    return new { ok = true, msg = "" };
                else
                    return new { ok = false, msg = ResourceUtils.GetString("2cf8b4eaefa665e42735c359a1072a9c", "Update failed!") };
            }
            else
                return new { ok = true, msg = "" };
        }

        public static async Task<dynamic> DeleteChannel(string id)
        {
            CommunicationServiceProxy csvc = new CommunicationServiceProxy();
            Communication c = await csvc.LoadEntityByKeyAsync(Cntx, id);
            if (c == null)
                return new { ok = false, msg = ResourceUtils.GetString("2d092e365fe439f4e11223a6aac685df", "Channel not found!") };
            await csvc.DeleteEntitiesAsync(Cntx, new CommunicationSet(), new Communication[] { c });
            return new { ok = true, msg = "" };
        }
    }
}
