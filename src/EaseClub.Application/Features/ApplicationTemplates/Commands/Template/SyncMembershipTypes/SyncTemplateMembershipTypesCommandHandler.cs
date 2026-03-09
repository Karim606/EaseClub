using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.SyncMembershipTypes
{
    public class SyncTemplateMembershipTypesCommandHandler(
    IApplicationTemplateRepository templateRepo,
    IMembershipTypeRepository membershipRepo,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SyncTemplateMembershipTypesCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(
            SyncTemplateMembershipTypesCommand request,
            CancellationToken cancellationToken)
        {
            var template = await templateRepo.GetTemplateWithConnectedMembershipTypeAsync(request.TemplateId);

            if (template == null)
                return Error.NotFound("Template.NotFound");

            var membershipTypes = await membershipRepo
                .GetByIdsAsync(request.MembershipTypeIds,cancellationToken);

            var result = template.SyncMembershipTypes(membershipTypes);

            if (result.IsError)
                return result.TopError;

            await unitOfWork.SaveChangesAsync();

            return Result.Success;
        }
    }
}
