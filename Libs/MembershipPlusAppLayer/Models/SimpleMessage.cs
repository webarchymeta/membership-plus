using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archymeta.Web.MembershipPlus.AppLayer.Models
{
    public enum MessagePriority
    {
        Background = 0,
        Batch = 1,
        Normal = 2,
        Urgent = 3,
        Critical = 4
    }

    public class SimpleMessage
    {
        public int TypeId;
        public string Id;
        public string Title;
        public string Message;
        public string Data;
        public MessagePriority Priority = MessagePriority.Normal;
    }
}
