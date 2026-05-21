using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Branches.Commands.CreateBranch
{
    public class CreateBranchCommandValidator:AbstractValidator<CreateBranchCommand>
    {
        public CreateBranchCommandValidator() { 
            RuleFor(RuleFor => RuleFor.ClubId)
                .NotEmpty().WithMessage("ClubId must not be empty");
            RuleFor(RuleFor => RuleFor.Name).Length(3, 100)
                .WithMessage("Branch name must be between 3 and 100 characters");
            RuleFor(RuleFor => RuleFor.Name)
                .NotEmpty().WithMessage("Branch name must not be empty");
        }
    }
}
