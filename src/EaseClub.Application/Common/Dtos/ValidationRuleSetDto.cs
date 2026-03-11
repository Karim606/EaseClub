using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Dtos
{
    public record ValidationRuleSetDto(
    [DefaultValue(true)] bool IsRequired,
    int? MinLength = null,
    int? MaxLength = null,
    string? Regex = null,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    DateTime? MinDate = null,
    DateTime? MaxDate = null
    );
}
