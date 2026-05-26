using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Payment.Queries.GetPaymentStatsByClub
{
    public record GetPaymentStatsByClubQuery(Guid ClubId) : IRequest<Result<PaymentStatsDto>>, IRequireClubAdmin;
}
