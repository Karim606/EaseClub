using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesByUser;
using EaseClub.Application.Features.Payment.Queries.GetInvoicesForClub;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{vesrion:ApiVersion}/invoices")]
    public class InvoicesController(ISender sender) : ApiController
    {

        [Authorize("ClubAdmin,SuperAdmin")]
        [HttpGet("/api/v{version:ApiVersion}/clubs/{clubId}/invoices")]

        [ProducesResponseType(typeof(UnifiedPaginatedResponse<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("GetInvoicesByClub")]
        [EndpointSummary("Get invoices for a club")]
        [EndpointDescription(
    "Retrieves a paginated list of invoices for a specific club.\n\n" +
    "Supports filtering and searching.\n\n" +
    "Filters:\n" +
    "- status: Issued | Paid | Void\n" +
    "- billingItemType: MembershipInstallment | EventRegistration\n\n" +
    "Search:\n" +
    "- invoiceId (Readable)\n" +
    "- billingItemId (Readable)\n\n" +
    "Pagination:\n" +
    "- Supports both Offset and Cursor pagination.\n" +
    "- Use 'page' & 'limit' OR 'cursor' & 'limit'."
)]
        public async Task<IActionResult> GetInvoicesByClub(Guid clubId,[FromQuery]GetInvoicesFilters filters, [FromQuery] PaginationRequest pagination, CancellationToken ct = default)
        {
            var result = await sender.Send(new GetInvoicesByClubQuery(clubId,filters,pagination),ct);
            
            return result.Match(
                success => Ok(success),
                Problem
            );
        }

        [Authorize("MemberUser,SuperAdmin")]
        [HttpGet]

        [ProducesResponseType(typeof(UnifiedPaginatedResponse<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]

        [EndpointName("GetInvoicesForMember")]
        [EndpointSummary("Get invoices for a member")]
        [EndpointDescription(
    "Retrieves a paginated list of invoices for a specific user.\n\n" +
    "Supports filtering and searching.\n\n" +
    "Filters:\n" +
    "- status: Issued | Paid | Void\n" +
    "- billingItemType: MembershipInstallment | EventRegistration\n\n" +
    "Search:\n" +
    "- invoiceId (Readable or Guid)\n" +
    "- billingItemId (Readable or Guid)\n\n" +
    "Security:\n" +
    "- MemberUser can only access their own invoices.\n" +
    "- SuperAdmin can access any user invoices.\n\n" +
    "Pagination:\n" +
    "- Supports Offset and Cursor pagination."
)]

        public async Task<IActionResult> GetInvoicesForMember([FromQuery] Guid userId,[FromQuery] GetInvoicesByUserFilters filters, [FromQuery] PaginationRequest pagination, CancellationToken ct = default)
        {
            var result = await sender.Send(new GetInvoicesByUserQuery(userId,filters, pagination), ct);

            return result.Match(
                success => Ok(success),
                Problem
            );
        }
    }
}
