using EaseClub.Domain.Common.Results;
using MediatR;
using System;

namespace EaseClub.Application.Features.Payment.Queries.GetInvoiceStatus
{
    public record GetInvoiceStatusQuery(Guid InvoiceId) : IRequest<Result<InvoiceStatusDto>>;

    public record InvoiceStatusDto(Guid InvoiceId, string Status, string ReadableId);
}
