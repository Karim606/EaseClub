using EaseClub.Application.Features.Auth.Common.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string unHashedRefreshToken) : IRequest<Result<AuthTokensDto>>;
}
