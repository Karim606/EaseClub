using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Commands.ToggleMembershipTypeActivation
{
    public class ToggleTypeActivationHandler(
    IMembershipTypeRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleTypeActivationCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ToggleTypeActivationCommand request, CancellationToken ct)
        {
            var membershipType = await repository.GetByIdAsync(request.Id, ct);

            if (membershipType is null)
            {
                return Error.NotFound("Membership type not found.");
            }

            // Logic: Flip the status
            if (membershipType.IsActive)
                membershipType.Deactivate();
            else
                membershipType.Activate();

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
