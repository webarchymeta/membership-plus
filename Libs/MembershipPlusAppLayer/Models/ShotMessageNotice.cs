using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer.Models
{
    public class ShotMessageNotice
    {
        public string msg;
        public string brief;
        public MemberNotificationType categ;
        public MemberCallback[] peers;
        public List<MemberCallback> callbacks;
    }

}
