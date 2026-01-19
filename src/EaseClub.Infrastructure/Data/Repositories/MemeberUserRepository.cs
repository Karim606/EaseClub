using EaseClub.Domain.Member;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class MemberUserRepository : EfRepository<MemberUser>,IMemberUserRepository
    {
        public MemberUserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<MemberUser> GetByEmailAsync(string email)
        {
           return await  _context.MemberUsers.FirstOrDefaultAsync(mu => mu.Email.Value == email);
        }
    }
}
