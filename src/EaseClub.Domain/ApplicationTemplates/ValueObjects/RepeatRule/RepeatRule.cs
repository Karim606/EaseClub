using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule
{
    public record RepeatRule
    {
        public RepeatMode Mode { get; init; }
        public int NumberOfRepeats { get; init; }

        private RepeatRule() { }
        private RepeatRule( int number,RepeatMode mode)
        {
            NumberOfRepeats = number;
            Mode = mode;
        }

        public static Result<RepeatRule> Create(int number, RepeatMode mode)
        {
            if (number <= 0)
                return RepeatErrors.NonPositiveRepeatCount;
            return new RepeatRule(number, mode);
        }

        public bool Evaluate(int actualInstances)
        {

            return Mode switch
            {
                RepeatMode.ExactValue => actualInstances == NumberOfRepeats,
                RepeatMode.AtLeastOne => actualInstances > 0 && actualInstances <= NumberOfRepeats,
                _ => false
            };

        }
    }
}
