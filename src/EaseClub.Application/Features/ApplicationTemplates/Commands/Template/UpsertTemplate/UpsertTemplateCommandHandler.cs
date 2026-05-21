using Microsoft.Extensions.Logging;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.ApplicationTemplates.SystemSections;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpsertTemplate
{
    public class UpsertTemplateCommandHandler(IApplicationTemplateRepository tempRepo,
        IUnitOfWork unitOfWork,
        ILogger<UpsertTemplateCommandHandler> logger) : IRequestHandler<UpsertTemplateCommand, Result<Success>>
    {

        public async Task<Result<Success>> Handle(UpsertTemplateCommand request, CancellationToken cancellationToken)
        {
            ApplicationTemplateDefinition template;

            // 1️⃣ Load existing template or create new
            if (request.TemplateId.HasValue)
            {
                template = await tempRepo.GetFullTemplateAsync(request.TemplateId.Value);

                if (template == null) { logger.LogError("NotFound error in UpsertTemplateCommandHandler: {Error}", Error.NotFound("Template.NotFound").ToLogObject()); return Error.NotFound("Template.NotFound"); }
                if (template.ClubId != request.ClubId) { logger.LogError("Forbidden error in UpsertTemplateCommandHandler: {Error}", Error.Forbidden("Template.Forbidden").ToLogObject()); return Error.Forbidden("Template.Forbidden"); }
                var updateResult = template.Update(request.Name);
                if (updateResult.IsError) { logger.LogError("Error in UpsertTemplateCommandHandler: {Error}", updateResult.TopError.ToLogObject()); return updateResult.TopError; }
            }
            else
            {
                var createResult = ApplicationTemplateDefinition.Create(
                    Guid.NewGuid(),
                    request.ClubId,
                    request.Name
                );
                if (createResult.IsError) { logger.LogError("Error in UpsertTemplateCommandHandler: {Error}", createResult.TopError.ToLogObject()); return createResult.TopError; }
                template = createResult.Value;
                await tempRepo.AddAsync(template);
            }

            // 2️⃣ Map to Snapshots
            var stepSnapshots = new List<StepSnapshot>();
            for (int sIndex = 0; sIndex < request.Steps.Count; sIndex++)
            {
                var sDto = request.Steps[sIndex];
                var sectionSnapshots = new List<SectionSnapshot>();
                
                for (int secIndex = 0; secIndex < sDto.Sections.Count; secIndex++)
                {
                    var secDto = sDto.Sections[secIndex];
                    var fieldSnapshots = new List<FieldSnapshot>();

                    // Validate system section integrity if applicable
                    if (secDto.Intent != SectionIntent.General)
                    {
                        var comparer = new SystemSectionIntegrityComparer();
                        var integrity = comparer.Validate(secDto.Intent, secDto.Fields.Select(f => f.ToFieldSpecification()).ToList());
                        if (integrity.IsError) return integrity.TopError;
                    }

                    for (int fIndex = 0; fIndex < secDto.Fields.Count; fIndex++)
                    {
                        var fDto = secDto.Fields[fIndex];
                        var validationRules = fDto.ValidationRules.ToDomain();
                        if (validationRules.IsError) return validationRules.TopError;

                        var isSystemField = secDto.Intent != SectionIntent.General && SystemSectionRegistry.ResolveKeys(fDto.Key, secDto.Intent);

                        fieldSnapshots.Add(new FieldSnapshot(
                            fDto.Id == Guid.Empty ? Guid.NewGuid() : fDto.Id,
                            string.IsNullOrWhiteSpace(fDto.Key) ? Guid.NewGuid().ToString() : fDto.Key, // Replace empty key with guid temporarily, the aggregate resolves it
                            fDto.Label,
                            fDto.FieldType,
                            ValidationRuleSetSnapshot.FromDomain(validationRules.Value),
                            null, // VisibilityCondition not mapped yet in this DTO
                            fDto.AllowedValues,
                            fIndex + 1,
                            isSystemField
                        ));
                    }

                    var repeatRule = secDto.RepeatRule?.ToDomain();
                    if (repeatRule != null && repeatRule.IsError) return repeatRule.TopError;

                    sectionSnapshots.Add(new SectionSnapshot(
                        secDto.Id == Guid.Empty ? Guid.NewGuid() : secDto.Id,
                        secDto.Title,
                        secIndex + 1,
                        repeatRule?.Value,
                        secDto.Intent,
                        fieldSnapshots
                    ));
                }

                stepSnapshots.Add(new StepSnapshot(
                    sDto.Id == Guid.Empty ? Guid.NewGuid() : sDto.Id,
                    sDto.Title,
                    sIndex + 1,
                    sectionSnapshots
                ));
            }

            var upsertResult = template.UpdateSteps(stepSnapshots);
            if (upsertResult.IsError) { logger.LogError("Error in UpsertTemplateCommandHandler: {Error}", upsertResult.TopError.ToLogObject()); return upsertResult.TopError; }
            
            var validation = template.ValidateConsistency();
            if (validation.IsError) { logger.LogError("Error in UpsertTemplateCommandHandler: {Error}", validation.TopError.ToLogObject()); return validation.TopError; }
            
            // 3️⃣ Persist changes
            await unitOfWork.SaveChangesAsync();

            return Result.Success;

        }
    }
}