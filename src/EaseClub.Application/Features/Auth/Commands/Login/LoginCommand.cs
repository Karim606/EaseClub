using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<Result<AuthTokensDto>>;
}
