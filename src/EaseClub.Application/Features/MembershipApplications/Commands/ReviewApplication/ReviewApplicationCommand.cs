using EaseClub.Application.Common;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.MembershipApplications.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Commands.ReviewApplication
{
    public record ReviewApplicationCommand(
        DecisionsAboutApplication Decision,
        string? RejectionReason = null
    //bool VisibleToUser = true
    ) : IRequest<Result<Success>>, IRequireClubOwnershipValidation
    {
        [JsonIgnore]
        public Guid ApplicationId { get; init; }
        public IEnumerable<OwnershipRule> Rules()
        {
            yield return new OwnershipRule(
                async (auth, clubId) => await auth.DoesResourceBelongToClubAsync<MembershipApplication>(ApplicationId, clubId),
                nameof(MembershipApplication),
                ApplicationId);
        }
    }
}
