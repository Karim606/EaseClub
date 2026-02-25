using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Queries;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EaseClub.Application.Features.MembershipApplications.Commands.CreateApplication
{
    public class CreateApplicationCommandHandler(
    IApplicationTemplateQueryService queryService,
    IMembershipPlanRepository membershipPlanRepository,
    IMembershipApplicationRepository applicationRepository,
    ICurrentUserService currentUserService,
    ILogger<CreateApplicationCommandHandler>logger,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateApplicationCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateApplicationCommand request, CancellationToken ct)
        {
            // 1. Fetch the Definition Tree (Relational)
            // Note: Use AsNoTracking and AsSplitQuery as we discussed for performance
            var template = await queryService.GetFullTemplateTreeAsync(request.TemplateId, ct);

            if (template == null)
            {
                logger.LogWarning("The template definition was not found"); 
                return Error.NotFound("Template.NotFound", "The template definition was not found.");
            }
            // 2. Serialize the Tree to JSON (The Snapshot)
            var snapshotJson = JsonSerializer.Serialize(template, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false
            });

            var plan = await membershipPlanRepository.GetByIdAsync(request.MembershipPlanId, ct);

            if(plan == null)
            {
                logger.LogWarning("membership plan was not found.");
                return Error.NotFound(description:"membership plan was not found.");
            }

            Guid.TryParse(currentUserService.GetId(),out var userId);


            // 3. Create the Aggregate Root
            var res =  MembershipApplication.Create(
                Guid.NewGuid(),
                "EaseClub"+ new Random().NextInt64(),
                snapshotJson,
                userId,
                request.ClubId,
                request.MembershipTypeId,
                request.MembershipPlanId,
                request.TemplateId,
                plan.TotalPrice

                );

            if (res.IsError)
            {
                logger.LogError("failed to create membership application, reason:{Error}", res.TopError);
                return res.TopError;
            }
            // 4. Persist
            await applicationRepository.AddAsync(res.Value, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return res.Value.Id;
        }
    }
}
