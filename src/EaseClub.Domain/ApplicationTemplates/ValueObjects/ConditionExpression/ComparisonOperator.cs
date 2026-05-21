using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression
{
    public enum ComparisonOperator
    {
        Equals = 0,
        NotEquals = 1,

        GreaterThan = 2,
        GreaterThanOrEqual = 3,

        LessThan = 4,
        LessThanOrEqual = 5,

        Contains = 6,
        In = 7
    }
}
