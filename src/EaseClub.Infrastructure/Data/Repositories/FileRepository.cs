using EaseClub.Domain.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class FileRepository : EfRepository<FileResource>, IFileRepository
    {
        public FileRepository(AppDbContext context) : base(context)
        {
        }
    }
}
