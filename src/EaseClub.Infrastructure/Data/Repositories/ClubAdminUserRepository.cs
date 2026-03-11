using EaseClub.Domain.ClubAdmin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class ClubAdminUserRepository:EfRepository<ClubAdminUser>,IClubAdminUserRepository
    {
        public ClubAdminUserRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ClubAdminUser> GetWithClubAsync(Guid userId, CancellationToken ct = default)
        {
           return await _context.ClubAdminUsers.Include(cau => cau.Club).FirstOrDefaultAsync(cau => cau.Id == userId,ct);
        }
    }
}
