using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Commands.CreateBranch
{
    public class CreateBranchCommandHandler(IBranchRepository branchRepository,IClubRepository clubRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateBranchCommandHandler> logger)
        : IRequestHandler<CreateBranchCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            var isExist = await clubRepository.IsExistAsync(request.ClubId);

            if (!isExist) {
                logger.LogWarning("Club with Id {ClubId} not found", request.ClubId);
                return Error.NotFound(description: $"Club with Id {request.ClubId} not found");
            }

            var result = Branch.Create(Guid.NewGuid(),request.ClubId,request.Name);
            
            if(result.IsSuccess)
            await branchRepository.AddAsync(result.Value);
            else {
                logger.LogWarning("Failed to create branch for Club with Id {ClubId}. Reason: {Reason}",
                    request.ClubId,result.TopError);
                return result.TopError;
            }
            await unitOfWork.SaveChangesAsync();
            return result.Value.Id;

        }
    }
}
