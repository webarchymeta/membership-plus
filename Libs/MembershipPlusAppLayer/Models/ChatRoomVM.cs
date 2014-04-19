using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace Archymeta.Web.MembershipPlus.AppLayer.Models
{
    public class ChatRoomVM
    {
        public string ID
        {
            get;
            set;
        }

        public bool DialogMode
        {
            get;
            set;
        }

        public bool RoomExists
        {
            get;
            set;
        }

        public string[] RoomPath
        {
            get;
            set;
        }

        public int Subscribers
        {
            get;
            set;
        }

        public bool IsSubscriber
        {
            get;
            set;
        }

        public bool ActiveNotifying
        {
            get;
            set;
        }

        public string[] ActivePeers
        {
            get;
            set;
        }

        public string[] RecentMsgs
        {
            get;
            set;
        }

        public string RoomInfo
        {
            get;
            set;
        }
    }
}
