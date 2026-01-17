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
        
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public void UpdateContactInfo(PhoneNumber phoneNumber, Email email)
        {
            PhoneNumber = phoneNumber;
            Email = email;
        }

        private ClubAdmin()
        {
        }
        private ClubAdmin(Guid id, string firstName, string lastName, PhoneNumber phoneNumber, Email email): base(id)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Email = email;
        }
        public static ClubAdmin Create(Guid id, string firstName, string lastName, PhoneNumber phoneNumber, Email email)
        {
            return new ClubAdmin(id, firstName,lastName, phoneNumber, email);
        }

    }
}
