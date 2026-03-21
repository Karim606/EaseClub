using EaseClub.Domain.MembershipApplications.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.ReviewApplication
{
    public class ReviewApplicationCommandValidator : AbstractValidator<ReviewApplicationCommand>
    {
        public ReviewApplicationCommandValidator()
        {
            RuleFor(x => x).Must(x => x.Decision == DecisionsAboutApplication.Approved || x.Decision == DecisionsAboutApplication.Rejected);
            RuleFor(x => x).Must(x => x.RejectionReason!=null).When(x => x.Decision == DecisionsAboutApplication.Rejected);
            RuleFor(x => x).Must(x => x.RejectionReason == null).When(x => x.Decision == DecisionsAboutApplication.Approved);
        }
    }
}
