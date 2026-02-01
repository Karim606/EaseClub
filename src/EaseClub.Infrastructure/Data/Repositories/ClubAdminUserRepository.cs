using EaseClub.Domain.ClubAdmin;
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
    }
}
