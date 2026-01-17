using EaseClub.Domain.Common;
using EaseClub.Domain.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EaseClub.Domain.User
{
    public class MemberUser:UserBase
    {


        private MemberUser ()
        {

        }

        private MemberUser (Guid id,string firstName,string lastName, PhoneNumber phoneNumber, Email email) : base(id, firstName, 
             lastName,  phoneNumber,  email)
        {
        
        }

        public static MemberUser Create (Guid id,string firstName,string lastName, PhoneNumber phoneNumber, Email email)
        {
            return new MemberUser(id,firstName,lastName,phoneNumber,email);
        }


    }
}
