using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Section.AddSection;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field.AddField
{
    public class AddFiedlCommandHandler(ILogger<AddFiedlCommandHandler> logger,
       IApplicationSectionRepository sectionRepository,
       IApplicationTemplateRepository tempRepo,
       IApplicationFieldRepository fieldRepository,
       IUnitOfWork unitOfWork)
   : IRequestHandler<AddFieldCommand, Result<Guid>>
    {


        public async Task<Result<Guid>> Handle(AddFieldCommand request, CancellationToken ct)
        {
            var temp = await tempRepo.GetFullTemplateAsync(request.TemplateId);

            if (temp == null)
                return Error.NotFound("ApplicationTemplate.NotFound", "The specified template does not exist.");

            var rulesOfValidation = request.ValidationRules;
            
            var rule = ValidationRuleSet.Create(rulesOfValidation.IsRequired,
                        rulesOfValidation.MinLength,
                        rulesOfValidation.MaxLength,
                        rulesOfValidation.Regex,
                        rulesOfValidation.MinValue,
                        rulesOfValidation.MaxValue);

            if (rule.IsError)
            {
                logger.LogError("cant create validation ruleset for new field,  request section id:{SectionId}, reason:{Error}.",
                    request.SectionId, 
                    rule.TopError);
                return rule.TopError;
            }

            ConditionExpression? visibilityCondition=null;
            if (request.VisibilityCondition != null) {
                
                var resOfCond = ConditionExpression.Create(
                   request.VisibilityCondition.DependsOnFieldKey,
                   request.VisibilityCondition.Operator,
                   request.VisibilityCondition.ExpectedValue
                 );

                if (resOfCond.IsError) {
                    logger.LogError("cant create visiblity condition for new field,  request section id:{SectionId}, reason:{Error}.",
                    request.SectionId,
                    resOfCond.TopError);
                    return resOfCond.TopError;
                }
                visibilityCondition = resOfCond.Value;
             }
            // 2. Domain Logic: Create the Field via the Section Aggregate
            var fieldResult = temp.AddFieldToSection(
                Guid.NewGuid(),
                request.SectionId,
                request.Key,
                request.label,
                request.type,
                rule.Value,
                visibilityCondition,
                request.PersistToMembership
            );

            if (fieldResult.IsError)
            {
                logger.LogError("Failed to add new field into section with id:{SectionId}, reason:{Error}", request.SectionId,
                    fieldResult.TopError.ToLogObject());
                return fieldResult.TopError;
            }

            await fieldRepository.AddAsync(fieldResult.Value);
            // 3. Persist
            await unitOfWork.SaveChangesAsync(ct);

            return fieldResult.Value.Id;
        }
    }
}
