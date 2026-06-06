using EaseClub.Domain.MembershipApplications.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications
{

    public sealed record ApplicationApprovedEvent(Guid ApplicationOwnerId,Guid ApplicationId,string TrackingNumber,Guid ReviewId,string? optionalNotes=null):DomainEvent;

    public sealed record ApplicationRejectedEvent(Guid ApplicationOwnerId, Guid ApplicationId,string TrackingNumber, Guid ReviewId, string RejectionReason):DomainEvent;

}
