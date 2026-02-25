using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class ApplicationFieldRepository : EfRepository<ApplicationFieldDefinition>, IApplicationFieldRepository
    {
        public ApplicationFieldRepository(AppDbContext context) : base(context)
        {
        }
    }
}
