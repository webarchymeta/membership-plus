using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer.Models
{
    public class NotificationType
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class NotificationTypes
    {
        public int SelectedIndex
        {
            get;
            set;
        }

        public NotificationType[] Types
        {
            get;
            set;
        }
    }
}