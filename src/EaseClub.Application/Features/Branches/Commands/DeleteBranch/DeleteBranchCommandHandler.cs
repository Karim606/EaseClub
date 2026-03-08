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
    public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result<Success>>
    {
        private readonly IBranchRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBranchCommandHandler(IBranchRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Success>> Handle(DeleteBranchCommand request, CancellationToken ct)
        {
            var branch = await _repository.GetByIdAsync(request.Id, ct);
            if (branch is null)
                return Error.NotFound("Branch not found in this club.");

            // Assuming you have a Delete method in your repository or aggregate
            await _repository.DeleteAsync(branch);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
