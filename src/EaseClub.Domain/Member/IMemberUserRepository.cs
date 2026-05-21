using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Member;
namespace EaseClub.Domain.Member
{
   
        public interface IMemberUserRepository:IRepository<MemberUser>
        {
            Task<MemberUser> GetByEmailAsync(string email);
            public Task<bool> PhoneExistsAsync(string phoneNumber);
            public Task<bool> EmailExistsAsync(string email);
        
        }

}
