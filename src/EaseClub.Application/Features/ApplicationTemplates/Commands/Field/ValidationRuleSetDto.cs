using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Field
{
    public class ValidationRuleSetDto
    {
        public bool IsRequired { get; init; } = true;
        public int? MinLength { get; init; }
        public int? MaxLength { get; init; }
        public string? Regex { get; init; }
        public decimal? MinValue { get; init; }
        public decimal? MaxValue { get; init; }
    }
}
