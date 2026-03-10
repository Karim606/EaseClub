using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Memberships.Events
{
    public sealed record MembershipCreatedDomainEvent(
        Guid MembershipId,
        Guid UserId,
        Guid ClubId,
        Guid MembershipTypeId,
        Guid MembershipPlanId,
        DateTime StartDate,
        DateTime EndDate
    ) : DomainEvent;

    public sealed record MembershipActivatedDomainEvent(
        Guid MembershipId,
        Guid UserId,
        DateTime ActivatedAt
    ) : DomainEvent;

    public sealed record MembershipCancelledDomainEvent(
        Guid MembershipId,
        Guid UserId,
        string? CancellationReason,
        DateTime CancelledAt
    ) : DomainEvent;

    public sealed record MembershipExpiredDomainEvent(
        Guid MembershipId,
        Guid UserId,
        DateTime ExpiredAt
    ) : DomainEvent;

    public sealed record MembershipRenewedDomainEvent(
        Guid MembershipId,
        Guid UserId,
        DateTime OldEndDate,
        DateTime NewEndDate,
        Guid? NewPlanId,
        DateTime RenewedAt
    ) : DomainEvent;

    public sealed record MembershipUpgradedDomainEvent(
        Guid MembershipId,
        Guid UserId,
        Guid OldMembershipTypeId,
        Guid NewMembershipTypeId,
        Guid OldPlanId,
        Guid NewPlanId,
        DateTime UpgradedAt
    ) : DomainEvent;

    public sealed record MembershipDowngradedDomainEvent(
        Guid MembershipId,
        Guid UserId,
        Guid OldMembershipTypeId,
        Guid NewMembershipTypeId,
        Guid OldPlanId,
        Guid NewPlanId,
        DateTime DowngradedAt
    ) : DomainEvent;

    public sealed record FamilyMemberAddedDomainEvent(
        Guid MembershipId,
        Guid FamilyMemberId,
        string FamilyMemberName,
        DateTime AddedAt
    ) : DomainEvent;

    public sealed record FamilyMemberRemovedDomainEvent(
        Guid MembershipId,
        Guid FamilyMemberId,
        string FamilyMemberName,
        DateTime RemovedAt
    ) : DomainEvent;
}
