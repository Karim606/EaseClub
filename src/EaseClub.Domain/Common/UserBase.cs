using EaseClub.Domain.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common
{
    public abstract class UserBase:AuditableEntity
    {
        public PhoneNumber PhoneNumber { get; private set; }
        public Email Email { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }


        protected UserBase()
        {

        }

        protected UserBase(Guid id, string firstName, string lastName, PhoneNumber phoneNumber, Email email) : base(id)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Email = email;
        }

      
    }
}
