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

namespace EaseClub.Application.Features.Branches.Commands.EditBranch
{
    public class EditBranchCommandHandler(IBranchRepository repository, IUnitOfWork unitOfWork,ILogger<EditBranchCommandHandler> logger) : IRequestHandler<EditBranchCommand, Result<Success>>
    {

        public async Task<Result<Success>> Handle(EditBranchCommand request, CancellationToken ct)
        {
            var branch = await repository.GetByIdAsync(request.Id, ct);
            if (branch is null) { logger.LogError("NotFound error in EditBranchCommandHandler: {Error}", Error.NotFound("Branch not found.").ToLogObject()); return Error.NotFound("Branch not found."); }
            var updateResult = branch.Update(request.Name, request.Address);
            if (updateResult.IsError) { logger.LogError("Error in EditBranchCommandHandler: {Error}", updateResult.TopError.ToLogObject()); return updateResult.TopError; }
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
