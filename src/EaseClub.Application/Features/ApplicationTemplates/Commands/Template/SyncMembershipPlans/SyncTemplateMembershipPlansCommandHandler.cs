using Microsoft.Extensions.Logging;
using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.MembershipTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.SyncMembershipTypes
{
    public class SyncTemplateMembershipPlansCommandHandler(IApplicationTemplateRepository templateRepo,
    IMembershipPlanRepository planRepo,
    IUnitOfWork unitOfWork,
        ILogger<SyncTemplateMembershipPlansCommandHandler> logger)
    : IRequestHandler<SyncTemplateMembershipPlansCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(
            SyncTemplateMembershipPlansCommand request,
            CancellationToken cancellationToken)
        {
            var template = await templateRepo.GetTemplateWithConnectedMembershipPlansAsync(request.TemplateId);

            if (template == null) { logger.LogError("NotFound error in SyncTemplateMembershipPlansCommandHandler: {Error}", Error.NotFound("Template.NotFound").ToLogObject()); return Error.NotFound("Template.NotFound"); }
            var membershipPlans = await planRepo
                .GetByIdsAsync(request.MembershipPlansIds,cancellationToken);

            var result = template.SyncMembershipPlans(membershipPlans);

            if (result.IsError) { logger.LogError("Error in SyncTemplateMembershipPlansCommandHandler: {Error}", result.TopError.ToLogObject()); return result.TopError; }
            await unitOfWork.SaveChangesAsync();

            return Result.Success;
        }
    }
}
