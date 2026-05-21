using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Dtos
{
    public record ConditionDto(
    string FieldKey,
    ComparisonOperator Operator,
    string ExpectedValue
    );


}
