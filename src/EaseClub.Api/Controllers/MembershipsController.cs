using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForAdmin;
using EaseClub.Application.Features.Memberships.Queries.GetInstallmentsForMember;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipDetails;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipsForAdmin;
using EaseClub.Application.Features.Memberships.Queries.GetMembershipsForMember;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EaseClub.Api.Controllers
{
    [Route("api/v{version:ApiVersion}/memberships")]
    public class MembershipsController(ISender sender) : ApiController
    {
        [Authorize(Roles = "Member,SuperAdmin")]
        [HttpGet("/api/v{version:ApiVersion}/{userId}/memberships")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(List<MembershipForMemberDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointName("GetMyMemberships")]
        [EndpointSummary("Retrieves the memberships for the authenticated member.")]
        [EndpointDescription(
            "Returns all memberships that belong to the current authenticated member user.\n\n" +
            "Security:\n" +
            "it support filteration with membership status, so members can easily find memberships in a specific state (e.g. active, expired).\n\n" +
            "- MemberUser sees their own memberships.\n" +
            "- SuperAdmin can call the endpoint in an authenticated context.\n\n" +
            "Each result includes the membership identifier, membership number, club name, and current status."
        )]
        public async Task<IActionResult> GetMyMemberships(Guid userId, MembershipStatus? status, CancellationToken ct)
        {
            var result = await sender.Send(new GetMembershipsForMemberQuery(userId, status), ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpGet("/api/v{version:ApiVersion}/clubs/{clubId:guid}/memberships")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(UnifiedPaginatedResponse<MembershipsAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointName("GetMembershipsForAdmin")]
        [EndpointSummary("Retrieves memberships for a specific club.")]
        [EndpointDescription(
            "Returns a paginated list of memberships for the specified club.\n\n" +
            "Each item includes member name, membership number, membership type, membership plan, " +
            "created date, and current status.\n\n" +
            "Pagination:\n" +
            "- Supports the unified pagination request used across the API."
        )]
        public async Task<IActionResult> GetForAdmin(Guid clubId, [FromQuery]MembershipStatus? status, [FromQuery] string? search, [FromQuery] PaginationRequest pagination, CancellationToken ct)
        {
            var result = await sender.Send(new GetMembershipsForAdminQuery(clubId, status, search, pagination), ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "Member,ClubAdmin,SuperAdmin")]
        [HttpGet("{membershipId:guid}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(MembershipDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointName("GetMembershipById")]
        [EndpointSummary("Retrieves the details for a specific membership.")]
        [EndpointDescription(
            "Returns the membership summary for a single membership, including membership number, " +
            "membership type, membership plan, created date, current period, and current status.\n\n" +
            "Access Rules:\n" +
            "- MemberUser can access memberships they own.\n" +
            "- ClubAdmin and SuperAdmin can access memberships they are authorized to manage."
        )]
        public async Task<IActionResult> GetById(Guid membershipId, CancellationToken ct)
        {
            var result = await sender.Send(new GetMembershipDetailQuery(membershipId), ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "Member,ClubAdmin,SuperAdmin")]
        [HttpGet("{membershipId:guid}/installments")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(List<InstallmentMemberDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointName("GetMembershipInstallments")]
        [EndpointSummary("Retrieves the installments for a specific membership.")]
        [EndpointDescription(
            "Returns the installments for the current cycle of a membership.\n\n" +
            "This endpoint is membership-focused, so `DueDate` is part of the contract here because " +
            "installments are the source of truth for scheduled payment obligations.\n\n" +
            "Access Rules:\n" +
            "- MemberUser can access memberships they own.\n" +
            "- ClubAdmin and SuperAdmin can access memberships they are authorized to manage."
        )]
        public async Task<IActionResult> GetInstallments(Guid membershipId, CancellationToken ct)
        {
            var result = await sender.Send(new GetInstallmentForMemberQuery(membershipId), ct);
            return result.Match(Ok, Problem);
        }

        [Authorize(Roles = "ClubAdmin,SuperAdmin")]
        [HttpGet("/api/v{version:ApiVersion}/clubs/{clubId:guid}/memberships/installments")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(UnifiedPaginatedResponse<InstallmentAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointName("GetClubMembershipInstallments")]
        [EndpointSummary("Retrieves membership installments for a club.")]
        [EndpointDescription(
            "Returns a paginated list of membership installments for a club.\n\n" +
            "Filters:\n" +
            "- `status`: filters installments by installment status.\n" +
            "- `search`: searches by installment readable ID or membership number.\n\n" +
            "Pagination:\n" +
            "- Supports the unified pagination request used across the API.\n\n" +
            "This endpoint intentionally exposes installment-specific fields such as `DueDate`, " +
            "because it is a membership installment query rather than a generic billing-item query."
        )]
        public async Task<IActionResult> GetClubInstallments(
            Guid clubId,
            [FromQuery] InstallmentStatus? status,
            [FromQuery] string? search,
            [FromQuery] PaginationRequest pagination,
            CancellationToken ct)
        {
            var result = await sender.Send(
                new GetInstallmentsForAdminQuery(clubId, status, search, pagination),
                ct);

            return result.Match(Ok, Problem);
        }
    }
}
