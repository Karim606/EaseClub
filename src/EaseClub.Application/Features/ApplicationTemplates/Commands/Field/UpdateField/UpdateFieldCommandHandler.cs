using EaseClub.Application.Common.Interfaces;
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

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field.UpdateField
{
    public class UpdateFieldCommandHandler(IApplicationFieldRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFieldCommandHandler>logger)
    : IRequestHandler<UpdateFieldCommand, Result<Success>>
    {
        public async Task<Result<Success>> Handle(UpdateFieldCommand request, CancellationToken ct)
        {
            var field = await repository.GetByIdAsync(request.FieldId, ct);
            if (field == null) return Error.NotFound("Field.NotFound", "Field not found.");

            var rulesOfValidation = request.ValidationRules;

            var rule = ValidationRuleSet.Create(rulesOfValidation.IsRequired,
                        rulesOfValidation.MinLength,
                        rulesOfValidation.MaxLength,
                        rulesOfValidation.Regex,
                        rulesOfValidation.MinValue,
                        rulesOfValidation.MaxValue);

            if (rule.IsError)
            {
                logger.LogError("updating field definiton failed" +
                    "cant update validation ruleset for new field,  request section id:{SectionId}, reason:{Error}.",
                    field.SectionId,
                    rule.TopError);
                return rule.TopError;
            }

            ConditionExpression? visibilityCondition = null;
            if (request.VisibilityCondition != null)
            {

                var resOfCond = ConditionExpression.Create(
                   request.VisibilityCondition.DependsOnFieldKey,
                   request.VisibilityCondition.Operator,
                   request.VisibilityCondition.ExpectedValue
                 );

                if (resOfCond.IsError)
                {
                    logger.LogError("updating field definiton failed" +
                        "cant update visiblity condition for new field,  request section id:{SectionId}, reason:{Error}.",
                        field.SectionId,
                        resOfCond.TopError);
                    return resOfCond.TopError;
                }
                visibilityCondition = resOfCond.Value;
            }

            var res = field.Update(
                        request.Label,
                        rule.Value,
                        visibilityCondition,
                        request.PersistToMembership,
                        request.AllowedValues
                        );
            if (res.IsError)
            {
                logger.LogError("Failed to update field with id:{FieldId}, reason:{Error}", request.FieldId, res.TopError);
                return res.TopError;
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
