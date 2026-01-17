using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Member;
namespace EaseClub.Domain.Member
{
   
        public interface IMemberUserRepository
        {
            Task AddAsync(MemberUser member);
            Task<MemberUser> GetByIdAsync(Guid id);
            Task UpdateAsync(MemberUser member);
            Task DeleteAsync(MemberUser member);
        }

}
