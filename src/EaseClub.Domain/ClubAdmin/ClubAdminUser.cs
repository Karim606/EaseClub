using EaseClub.Domain.Clubs;
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
        public Guid ClubId { get; private set; }
        public Club Club { get; private set; }
        private ClubAdminUser()
        {
        }
        private ClubAdminUser(Guid id,Guid clubId, string firstName, string lastName, PhoneNumber phoneNumber, Email email):
            base(id, firstName, lastName,
             phoneNumber,  email)
        {
            ClubId = clubId;
        }
        public static ClubAdminUser Create(Guid id,Guid clubId, string firstName, string lastName, PhoneNumber phoneNumber, Email email)
        {
            return new ClubAdminUser(id,clubId, firstName,lastName, phoneNumber, email);
        }

    }
}
