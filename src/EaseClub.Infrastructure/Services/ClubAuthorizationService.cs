using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services
{


    public class ClubAuthorizationService : IClubAuthorizationService
    {
        private readonly AppDbContext _context;

        public ClubAuthorizationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserAdminOfClubAsync(Guid userId, Guid clubId)
        {
            return await _context.ClubAdminUsers
                .AnyAsync(x => x.Id == userId && x.ClubId == clubId);
        }

        public async Task<bool> DoesResourceBelongToClubAsync<TEntity>(
            Guid entityId,
            Guid clubId) where TEntity : class,IHaveClub
        {
            return await _context.Set<TEntity>()
                .AnyAsync(x => x.ClubId == clubId && x.Id == entityId);
        }
    }

}
