using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidatior : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidatior()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(10);
            RuleFor(x => x.Password).Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.");
            RuleFor(x => x.Password).Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.");
            RuleFor(x => x.Password).Matches("[0-9]").WithMessage("Password must contain at least one number.");
            RuleFor(x => x.FirstName).NotEmpty().MinimumLength(3).MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MinimumLength(3).MaximumLength(50);
        }
    }
}
