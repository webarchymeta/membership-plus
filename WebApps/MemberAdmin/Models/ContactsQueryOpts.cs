using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MemberAdminMvc5.Models
{
    public class ContactType
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }

    public class ContactsQueryOpts
    {
        public string UserId
        {
            get;
            set;
        }

        public int TypeId
        {
            get;
            set;
        }

        public bool Outgoing
        {
            get;
            set;
        }

        public ContactType[] Types
        {
            get;
            set;
        }

    }
}