using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Commands.CreateMembershipType
{
    public class CreateMembershipTypeCommandValidator:AbstractValidator<CreateMembershipTypeCommand>
    {
        public CreateMembershipTypeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Membership type name is required.")
                .MaximumLength(150).WithMessage("Membership type name must not exceed 150 characters.");

            When(x => !x.AllBranchesPermitted, () =>
            {
                RuleFor(x => x.BranchIds)
                    .NotEmpty().WithMessage("At least one branch ID must be specified when not all branches are permitted.");
            });
        }
    }
}
