using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.Register
{
    public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
    ) : IRequest<Result<AuthTokensDto>>;
}
