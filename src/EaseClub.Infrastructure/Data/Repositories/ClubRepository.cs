using EaseClub.Domain.Clubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class ClubRepository:EfRepository<Club>,IClubRepository
    {
        public ClubRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Clubs
                .Include(c => c.Logo)
                .Include(c => c.CoverImage)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }
    }
}
