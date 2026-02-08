using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipTypes
{
    public class MembershipType:AuditableEntity,IHaveClub
    {
        public Guid ClubId { get; private set; }

        public string Name { get; private set; }
        public string? Description { get; private set; }

        public bool FamilyAllowed { get; private set; }
        public int? MaxFamilyMembers { get; private set; }

        public bool AllBranchesPermitted { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<MembershipTypeBranch> _PermittedBranches = new();
        public IReadOnlyList<MembershipTypeBranch> PermittedBranches => _PermittedBranches.AsReadOnly();

        private readonly List<MembershipPlan> _Plans = new();
        public IReadOnlyList<MembershipPlan> Plans => _Plans.AsReadOnly();

        public void AddPlan(MembershipPlan plan)
        {
            _Plans.Add(plan);
        }

        public void RemovePlan(MembershipPlan plan)
        {
            _Plans.Remove(plan);
        }

        private MembershipType() { } 

        private MembershipType(Guid id,Guid clubId, string name):base(id)
        {
            ClubId = clubId;
            Name = name;
            IsActive = true;
            AllBranchesPermitted = true;
        }

        public static Result<MembershipType> Create(Guid id,Guid clubId, string name)
        {
            if (clubId == Guid.Empty)
                return MembershipTypeErrors.ClubIdIsRequired;

            if (string.IsNullOrWhiteSpace(name))
                return MembershipTypeErrors.MembershipTypeNameMustNotBeEmpty;

            return new MembershipType(id,clubId, name);
        }

        public Result<Success> EnableFamily(int maxFamilyMembers)
        {
            if (maxFamilyMembers <= 0)
                return MembershipTypeErrors.MaxFamilyMembersMustBeGreaterThanZero;

            FamilyAllowed = true;
            MaxFamilyMembers = maxFamilyMembers;
            return Result.Success;
        }

        public void DisableFamily()
        {
            FamilyAllowed = false;
            MaxFamilyMembers = null;
        }

        public Result<Success> RestrictToBranches(IEnumerable<Guid> branchIds)
        {
            var ids = branchIds?.Distinct().ToList();

            if (ids == null || ids.Count == 0)
                return MembershipTypeErrors.RestrictedToBranchListMustBeGreaterThanZero;

            AllBranchesPermitted = false;
            _PermittedBranches.Clear();

            foreach (var id in ids)
            {
                var type = MembershipTypeBranch.Create(Id, id);

                if (type.IsSuccess)
                    _PermittedBranches.Add(type.Value);

                else return type.TopError;
            }

            return Result.Success;
        }

        public Result<Success> PermitAllBranchesAccess()
        {
            if (AllBranchesPermitted)
                return MembershipTypeErrors.MembershipTypeAlreadyAllBranchesPermitted;

            AllBranchesPermitted = true;
            _PermittedBranches.Clear();
            return Result.Success;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        public Result<Success> UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return MembershipTypeErrors.MembershipTypeNameMustNotBeEmpty;

            Name = name;
            return Result.Success;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }
    }

}
