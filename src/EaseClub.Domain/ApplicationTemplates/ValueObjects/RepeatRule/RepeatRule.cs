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
        public string DependsOnFieldCode { get; init; }
        public RepeatMode Mode { get; init; }

        private RepeatRule(string fieldCode, RepeatMode mode)
        {
            DependsOnFieldCode = fieldCode;
            Mode = mode;
        }

        public static Result<RepeatRule> Create(string fieldCode, RepeatMode mode)
        {
            if (string.IsNullOrWhiteSpace(fieldCode))
                return RepeatErrors.FieldCodeRequired;

            return new RepeatRule(fieldCode, mode);
        }

        public int Evaluate(string? actualValue)
        {
            if (!int.TryParse(actualValue, out var number))
                return 0;

            return Mode switch
            {
                RepeatMode.ExactValue => number,
                RepeatMode.AtLeastOne => Math.Max(1, number),
                RepeatMode.None => 0,
                _ => 0
            };
        }
    }
}
