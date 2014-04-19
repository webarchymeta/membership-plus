using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer.Models
{
    public class ChatContextVM
    {
        public string[] MemberIds
        {
            get;
            set;
        }

        public List<EntityAbs<UserGroup>> TopRooms
        {
            get;
            set;
        }
    }
}
