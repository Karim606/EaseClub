using EaseClub.Domain.Common;
using EaseClub.Domain.Member;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class UserBaseRepository : EfRepository<UserBase>, IUserBaseRepository
    {
        public UserBaseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> PhoneExistsAsync(string phoneNumber)
        {
            return await _context.UsersBase.AnyAsync(u => u.PhoneNumber.Value == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.UsersBase.AnyAsync(u => u.Email.Value == email);
        }

        public async Task<UserBase> GetByEmailAsync(string email)
        {
            return await _context.UsersBase.FirstOrDefaultAsync(u => u.Email.Value == email);
        }
    }
}
