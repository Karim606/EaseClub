using EaseClub.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships
{
    public class MembershipNumberGenerator
    {
        private readonly IUnitOfWork _unitOfWork;

        public MembershipNumberGenerator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateAsync(string clubCode)
        {
            var sequence = await _unitOfWork.GetNextMembershipSequenceAsync();
            var year = DateTime.UtcNow.ToString("yy");

            return $"{clubCode}-{year}-{sequence:D6}";
        }
    }
}
