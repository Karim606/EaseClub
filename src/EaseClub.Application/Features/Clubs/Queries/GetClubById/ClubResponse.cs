using EaseClub.Domain.Branches;
using EaseClub.Domain.Clubs.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Clubs.Queries.GetClubById
{
    public sealed class ClubResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string About { get; init; } = string.Empty;
        public string LogoUrl { get; init; } = string.Empty;
        public string CoverImageUrl { get; init; } = string.Empty;
        public string ImageUrl => CoverImageUrl;
        public Guid? LogoId { get; init; }
        public Guid? CoverImageId { get; init; }
        public List<Amenity>Amenities { get; init; } = new List<Amenity>();
        public List<WorkSchedule> WorkSchedules { get; init; } = new List<WorkSchedule>();
        public ContactInfo ContactInfo { get; init; }

        // public BranchResponse[] Branches { get; init; } = Array.Empty<BranchResponse>();
    }
}
