using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates
{
    public class GetInstallmentTemplatesByClubValidator:AbstractValidator<GetInstallmentTemplatesByClubQuery>
    {
        public GetInstallmentTemplatesByClubValidator() {

            RuleFor(x => x) 
              .Must(x => x.ClubId.HasValue || x.PlanId.HasValue)
              .WithMessage("At least one filter must be provided: clubId or planId.");
        }
    }
}
