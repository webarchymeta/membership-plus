using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archymeta.Web.MembershipPlus.AppLayer.Models
{
    public class BinaryRecord
    {
        public byte[] Data
        {
            get;
            set;
        }

        public string MimeType
        {
            get;
            set;
        }

        public DateTime LastModified
        {
            get;
            set;
        }
    }
}
