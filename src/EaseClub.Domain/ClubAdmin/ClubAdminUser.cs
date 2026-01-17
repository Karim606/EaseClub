using EaseClub.Domain.Common;
using EaseClub.Domain.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ClubAdmin
{
    public class ClubAdminUser:UserBase 
    {
        
        private ClubAdminUser()
        {
        }
        private ClubAdminUser(Guid id, string firstName, string lastName, PhoneNumber phoneNumber, Email email): base(id, firstName, lastName,
             phoneNumber,  email)
        {
          
        }
        public static ClubAdminUser Create(Guid id, string firstName, string lastName, PhoneNumber phoneNumber, Email email)
        {
            return new ClubAdminUser(id, firstName,lastName, phoneNumber, email);
        }

    }
}
