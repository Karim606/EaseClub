using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Payment.Enums;
using MediatR;
using System;

namespace EaseClub.Application.Features.Payment.Commands.IssueInvoice
{
    public record IssueInvoiceCommand(Guid BillingItemId, BillingItemType Type) : IRequest<Result<Guid>>;
    public record IssueInvoiceRequest(Guid BillingItemId, BillingItemType Type);

}
