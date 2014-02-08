using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;
using Archymeta.Web.MembershipPlus.AppLayer.Models;

namespace Archymeta.Web.MembershipPlus.AppLayer
{
    public class MembershipContext
    {
        public static async Task<BinaryRecord> GetMemberIcon(string userName)
        {
            UserServiceProxy usvc = new UserServiceProxy();
            var ul = await usvc.LoadEntityByNatureAsync(ApplicationContext.ClientContext, userName);
            if (ul == null || ul.Count == 0)
                return null;
            return await GetMemberIconById(ul[0].ID);
        }

        public static async Task<BinaryRecord> GetMemberIconById(string id)
        {
            UserAppMemberServiceProxy umsvc = new UserAppMemberServiceProxy();
            var um = await umsvc.LoadEntityByKeyAsync(ApplicationContext.ClientContext, ApplicationContext.App.ID, id);
            if (um == null)
                return null;
            BinaryRecord rec = new BinaryRecord();
            rec.MimeType = um.IconMime;
            rec.LastModified = um.IconLastModified.HasValue ? um.IconLastModified.Value : DateTime.MaxValue;
            rec.Data = await umsvc.LoadEntityIconImgAsync(ApplicationContext.ClientContext, ApplicationContext.App.ID, id);
            return rec;
        }
    }
}
