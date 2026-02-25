using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field
{
    public class ConditionExpressionDto
    {
        public string DependsOnFieldKey { get; init; }
        public ComparisonOperator Operator { get; init; }
        public string ExpectedValue { get; init; }
    }
}
