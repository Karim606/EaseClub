using Microsoft.Extensions.Logging;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForMember
{
    public class GetInstallmentForMemberQueryHandler(IMembershipRepository membershipRepository,
        ILogger<GetInstallmentForMemberQueryHandler> logger) : IRequestHandler<GetInstallmentForMemberQuery, Result<List<InstallmentMemberDto>>>
    {
        public async Task<Result<List<InstallmentMemberDto>>> Handle(GetInstallmentForMemberQuery request, CancellationToken cancellationToken)
        {
            
            var installments = await membershipRepository.GetInstallmentsForCurrentCycleAsync(request.MembershipId, cancellationToken);

            var installmentDtos = installments.Select(installments => new InstallmentMemberDto
            (
                installments.Id,
                installments.Amount,
                installments.DueDate,
                installments.Status.ToString()
            )).ToList();

            return installmentDtos;
        }
    }
}
