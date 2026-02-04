using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipTypes.Commands.CreateMembershipType
{
    public record CreateMembershipTypeCommand(string Name,string? Description,bool FamilyAllowed, int? MaxFamilyMembers,
    bool AllBranchesPermitted, List<Guid> BranchIds) : IRequest<Result<Guid>>, IRequireClubAdmin
    {
        [JsonIgnore]
        public Guid ClubId { get; init; }
    }
}
