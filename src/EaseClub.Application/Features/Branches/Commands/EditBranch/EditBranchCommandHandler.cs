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
    public class EditBranchCommandHandler : IRequestHandler<EditBranchCommand, Result<Success>>
    {
        private readonly IBranchRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public EditBranchCommandHandler(IBranchRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Success>> Handle(EditBranchCommand request, CancellationToken ct)
        {
            var branch = await _repository.GetByIdAsync(request.Id, ct);
            if (branch is null) return Error.NotFound("Branch not found.");

            var updateResult = branch.Update(request.Name, request.Address);
            if (updateResult.IsError) return updateResult.TopError;

            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
