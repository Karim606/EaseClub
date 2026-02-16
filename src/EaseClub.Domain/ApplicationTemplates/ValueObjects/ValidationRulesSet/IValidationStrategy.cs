using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet
{
    public interface IValidationStrategy
    {
        // Returns an Error if invalid, null if valid
        Error? Validate(string? value, ValidationRuleSet rules);
    }
}
