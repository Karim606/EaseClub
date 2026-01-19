using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.Register
{
    public sealed record RegisterUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
);
}
