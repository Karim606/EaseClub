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

namespace EaseClub.Domain.Clubs
{
    public class Club:AuditableEntity
    {
        private Club()
        {
        }
        private Club(Guid id, string name): base(id)
        {
            Name = name;
        }
        
        public string Name { get; private set; }
        public bool IsActive { get; private set; } = true;

        private readonly List<Branch> _Branches = new List<Branch>();
        public IReadOnlyList<Branch> Branches => _Branches.AsReadOnly();

        private readonly List<ClubAdminUser> _ClubAdmins = new List<ClubAdminUser>();
        public IReadOnlyList<ClubAdminUser> ClubAdmins => _ClubAdmins.AsReadOnly();

        public static Result<Club> Create(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ClubErrors.NullOrWhiteSpaces;
            }

            if (name.Length < 3 || name.Length > 100)
            {
                return ClubErrors.Name_Length_NotSuitable;
            }

            return new Club(id, name);
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

    }
}
