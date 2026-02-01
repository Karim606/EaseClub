using EaseClub.Domain.Clubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class ClubRepository:EfRepository<Club>,IClubRepository
    {
        public ClubRepository(AppDbContext context) : base(context)
        {
        }
    }
}
