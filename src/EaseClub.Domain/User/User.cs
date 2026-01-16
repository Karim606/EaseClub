using EaseClub.Domain.Common;
using EaseClub.Domain.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.User
{
    public class User:AuditableEntity
    {
        public PhoneNumber PhoneNumber { get; private set; }
        public Email Email { get; private set; }

        private User ()
        {

        }

        private User (Guid id, PhoneNumber phoneNumber, Email email) : base(id)
        {
            PhoneNumber = phoneNumber;
            Email = email;
        }

        public static User Create (Guid id, PhoneNumber phoneNumber, Email email)
        {
            return new User(id, phoneNumber, email);
        }


    }
}
