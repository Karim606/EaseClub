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
        public string FirstName { get; private set; }
        public string LastName { get; private set; }


        private User ()
        {

        }

        private User (Guid id,string firstName,string lastName, PhoneNumber phoneNumber, Email email) : base(id)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Email = email;
        }

        public static User Create (Guid id,string firstName,string lastName, PhoneNumber phoneNumber, Email email)
        {
            return new User(id,firstName,lastName,phoneNumber,email);
        }


    }
}
