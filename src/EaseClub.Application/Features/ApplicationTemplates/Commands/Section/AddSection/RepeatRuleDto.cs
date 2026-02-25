using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Section.AddSection
{
    public class RepeatRuleDto
    {
        public string DependsOnFieldKey { get; init; }
        public RepeatMode Mode { get; init; }
    }
}
