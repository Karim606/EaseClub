using EaseClub.Domain.MembershipTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class MembershipTypeRepository : EfRepository<MembershipType>, IMembershipTypeRepository
    {
        public MembershipTypeRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<MembershipType> GetByClubIdAndNameAsync(Guid clubId, string name)
        {
            return await _context.MembershipTypes
                .FirstOrDefaultAsync(mt => mt.ClubId == clubId && mt.Name == name);
        }



        public async Task<List<MembershipType>> GetByIdsAsync(IEnumerable<Guid> ids,CancellationToken ct = default)
        {
            return await _context.MembershipTypes
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(ct);
        }

        public async Task<List<MembershipType>> GetTypesAsync(Guid? clubId, Guid? branchId, bool? allPermitted, bool? isActive, CancellationToken ct = default)
        {
            var membershipTypes = _context.MembershipTypes.AsQueryable();

            if(clubId.HasValue) membershipTypes = membershipTypes.Where(mt => mt.ClubId == clubId.Value);

            if (branchId.HasValue) membershipTypes = membershipTypes.Where(mt => mt.PermittedBranches.Any(pb => pb.BranchId == branchId.Value));

            if (allPermitted.HasValue) membershipTypes = membershipTypes.Where(mt => mt.AllBranchesPermitted == allPermitted.Value);

            if(isActive.HasValue) membershipTypes = membershipTypes.Where(mt => mt.IsActive == isActive.Value);

             return await membershipTypes.ToListAsync(ct);
        }
    }
}
