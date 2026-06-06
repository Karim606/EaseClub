using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Commands.ToggleBranchStatus
{
    public class ToggleBranchStatusCommandHandler(
        IBranchRepository repository, 
        IUnitOfWork unitOfWork,
        ILogger<ToggleBranchStatusCommandHandler> logger) 
        : IRequestHandler<ToggleBranchStatusCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(ToggleBranchStatusCommand request, CancellationToken ct)
        {
            var branch = await repository.GetByIdAsync(request.Id, ct);
            if (branch is null)
            {
                logger.LogError("NotFound error in ToggleBranchStatusCommandHandler: Branch {BranchId} not found.", request.Id);
                return Error.NotFound("Branch not found.");
            }

            if (branch.IsActive)
                branch.Deactivate();
            else
                branch.Activate();

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
