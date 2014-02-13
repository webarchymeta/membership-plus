using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Archymeta.Web.MembershipPlus.AppLayer.Models
{
    public class ContentRecord
    {
        public byte[] Data
        {
            get;
            set;
        }

        public string ETag
        {
            get
            {

                if (Data == null || Data.Length == 0)
                    return null;
                if (_etag == null)
                {
                    var h = HashAlgorithm.Create("MD5");
                    _etag = Convert.ToBase64String(h.ComputeHash(Data));
                }
                return _etag;
            }
        }
        private string _etag = null;

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
