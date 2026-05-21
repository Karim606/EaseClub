using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression
{
    public interface IComparisonStrategy
    {
        ComparisonOperator Operator { get; }
        bool Evaluate(string? actualValue, string expectedValue);
    }
}
