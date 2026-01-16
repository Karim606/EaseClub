using EaseClub.Domain.Common;
using EaseClub.Domain.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ClubAdmin
{
    public class ClubAdmin:AuditableEntity 
    {
        public PhoneNumber PhoneNumber { get; private set; }
        public Email Email { get; private set; }

        private ClubAdmin()
        {
        }
        private ClubAdmin(Guid id, PhoneNumber phoneNumber, Email email): base(id)
        {
            PhoneNumber = phoneNumber;
            Email = email;
        }
        public static ClubAdmin Create(Guid id, PhoneNumber phoneNumber, Email email)
        {
            return new ClubAdmin(id, phoneNumber, email);
        }

    }
}
