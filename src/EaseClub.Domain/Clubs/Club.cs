using EaseClub.Domain.Clubs;
using EaseClub.Domain.Branches;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Memberships;
using EaseClub.Domain.Clubs.ValueObjects;

namespace EaseClub.Domain.Clubs
{
    public class Club:AuditableEntity
    {
        private Club()
        {
        }
        private Club(Guid id, string name, string about, ContactInfo contactInfo, IEnumerable<WorkSchedule> workSchedules, IEnumerable<Amenity> amenities, string? logo = null, string? coverImage = null): base(id)
        {
            Name = name;
            About = about;
            ContactInfo = contactInfo;
            _workSchedules = workSchedules.ToList();
            _amenities = amenities.ToList();
            LogoUrl = logo;
            CoverImageUrl = coverImage;
        }
        
        public string Name { get; private set; }
        public bool IsActive { get; private set; } = true;

        private readonly List<Branch> _Branches = new List<Branch>();
        public IReadOnlyList<Branch> Branches => _Branches.AsReadOnly();

        private readonly List<ClubAdminUser> _ClubAdmins = new List<ClubAdminUser>();
        public IReadOnlyList<ClubAdminUser> ClubAdmins => _ClubAdmins.AsReadOnly();

        private readonly List<Membership> _Memberships = new List<Membership>();
        public IReadOnlyList<Membership> Memberships => _Memberships.AsReadOnly();
        public string? LogoUrl { get; private set; } // Reference to your FileResource
        public string? CoverImageUrl { get; private set; } // Reference to your FileResource
        public ContactInfo ContactInfo { get; private set; }

        private readonly List<WorkSchedule> _workSchedules = new();
        public IReadOnlyCollection<WorkSchedule> WorkSchedules => _workSchedules.AsReadOnly();

        private readonly List<Amenity> _amenities = new();
        public IReadOnlyCollection<Amenity> Amenities => _amenities.AsReadOnly();

        public string About { get; private set; }

        public static Result<Club> Create(Guid id, string name, string about, ContactInfo contactInfo, IEnumerable<WorkSchedule> workSchedules, IEnumerable<Amenity> amenities, string? logo = null, string? coverImage = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ClubErrors.NullOrWhiteSpaces;
            }

            if (name.Length < 3 || name.Length > 100)
            {
                return ClubErrors.Name_Length_NotSuitable;
            }

            
            return new Club(id, name, about, contactInfo, workSchedules, amenities, logo, coverImage);
        }

        public void SetWorkSchedules(IEnumerable<WorkSchedule> schedules)
        {
            _workSchedules.Clear();
            _workSchedules.AddRange(schedules);

        }

        public void UpdateDetails(
        string about,
        ContactInfo contact,
        IEnumerable<WorkSchedule> schedules,
        IEnumerable<Amenity> amenities,
        string? logoUrl,
        string? coverImageUrl)
        {
            About = about;
            ContactInfo = contact;
            SetWorkSchedules(schedules);
            _amenities.Clear();
            _amenities.AddRange(amenities);
            LogoUrl = logoUrl;
            CoverImageUrl = coverImageUrl;
        }
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

    }
}
