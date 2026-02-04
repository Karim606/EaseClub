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

            When(x => x.FamilyAllowed, () =>
            {
                RuleFor(x => x.MaxFamilyMembers)
                    .NotNull().WithMessage("Max family members is required when family membership is allowed.")
                    .GreaterThan(0).WithMessage("Max family members must be greater than zero.");
            });
            When(x => !x.AllBranchesPermitted, () =>
            {
                RuleFor(x => x.BranchIds)
                    .NotEmpty().WithMessage("At least one branch ID must be specified when not all branches are permitted.");
            });
        }
    }
}
