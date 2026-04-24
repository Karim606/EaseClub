using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Commands.DeleteBranch
{
    public class DeleteBranchCommandHandler(IBranchRepository repository, IUnitOfWork unitOfWork,ILogger<DeleteBranchCommandHandler>logger) : IRequestHandler<DeleteBranchCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(DeleteBranchCommand request, CancellationToken ct)
        {
            var branch = await repository.GetByIdAsync(request.Id, ct);
            if (branch is null) { logger.LogError("NotFound error in DeleteBranchCommandHandler: {Error}", Error.NotFound("Branch not found in this club.").ToLogObject()); return Error.NotFound("Branch not found in this club."); }

            // Assuming you have a Delete method in your repository or aggregate
            await repository.DeleteAsync(branch);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
