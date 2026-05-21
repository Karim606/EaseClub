using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Branches
{
    public class Branch : AuditableEntity,IHaveClub
    {
        private Branch()
        {
        }
        private Branch(Guid id, Guid clubId, string name, string address) : base(id)
        {
            ClubId = clubId;
            Name = name;
            Address = address;
        }
        public Guid ClubId { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; } = true;
        public string Address { get; private set; }
        public Club Club { get; private set; }

        private readonly List<MembershipTypeBranch> _MembershipTypeBranches = new();
        public IReadOnlyList<MembershipTypeBranch> MembershipTypeBranchesList => _MembershipTypeBranches.AsReadOnly();

        public static Result<Branch> Create(Guid id, Guid clubId, string name, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BranchErrors.NullOrWhiteSpaces;
            }

            if (name.Length < 3 || name.Length > 100)
            {
                return BranchErrors.Name_Length_NotSuitable;
            }

            return new Branch(id, clubId, name, address);
        }

        public  Result<Success> Update(string name, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BranchErrors.NullOrWhiteSpaces;
            }

            if (name.Length < 3 || name.Length > 100)
            {
                return BranchErrors.Name_Length_NotSuitable;
            }

            Name = name;
            Address = address;

            return Result.Success;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

    }
}
