using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.PricingPolices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace EaseClub.Domain.ApplicationTemplates
{
    public class ApplicationTemplateDefinition : AuditableEntity,IHaveClub
    {
        // 1. EF Core Constructor
        private ApplicationTemplateDefinition() { }

        // 2. Private Constructor for strict creation
        private ApplicationTemplateDefinition(Guid id, Guid clubId, string name) : base(id)
        {
            ClubId = clubId;
            Name = name;
        }

        public Guid ClubId { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool SupportsFamilyPlans { get; private set; } = false;

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        //private readonly List<ApplicationFieldDefinition> _Fields = new();
        //public  IReadOnlyList<ApplicationFieldDefinition> Fields => _Fields.AsReadOnly();

        private readonly List<StepSnapshot> _Steps = new();
        public IReadOnlyList<StepSnapshot> Steps => _Steps.AsReadOnly();

        private readonly List<MembershipPlan> _ConnectedMembershipPlans = new();
        public IReadOnlyList<MembershipPlan> ConnectedMembershipPlans => _ConnectedMembershipPlans.AsReadOnly();

        private readonly List<PricingPolicyAssignment> _PricingPolicyAssignments = new();
        public IReadOnlyList<PricingPolicyAssignment> PricingPolicyAssignments  => _PricingPolicyAssignments.AsReadOnly();

        private HashSet<string>? _fieldKeys;

        public HashSet<string> FieldKeys
        {
            get
            {
                if (_fieldKeys == null)
                {
                    _fieldKeys = new HashSet<string>(
                        _Steps
                        .SelectMany(s => s.Sections)
                        .SelectMany(sec => sec.Fields)
                        .Select(f => f.Key)
                    );
                }
                return _fieldKeys;
            }
        }

        // 3. Public Static Factory (Public because App Layer uses this)
        public static Result<ApplicationTemplateDefinition> Create(Guid id, Guid clubId, string name)
        {
            if (clubId == Guid.Empty) return ApplicationTemplateDefinitionErrors.ClubIdRequired;
            if (string.IsNullOrEmpty(name)) return ApplicationTemplateDefinitionErrors.InvalidName;

            return new ApplicationTemplateDefinition(id, clubId, name);
        }

        public Result<Success> Update(string name)
        {
            if (string.IsNullOrEmpty(name)) return ApplicationTemplateDefinitionErrors.InvalidName;
            
            Name = name;
            return Result.Success;
        }

        public Result<Success> UpdateSteps(List<StepSnapshot> newSteps)
        {
            // Validate unique keys
            var allKeys = newSteps
                .SelectMany(s => s.Sections)
                .SelectMany(sec => sec.Fields)
                .Select(f => f.Key)
                .ToList();

            var duplicateKeys = allKeys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateKeys.Any())
            {
                return Error.Conflict("Template.DuplicateKey", $"Duplicate keys found: {string.Join(", ", duplicateKeys)}");
            }

            // Validate at most one family section
            var familySectionCount = newSteps
                .SelectMany(s => s.Sections)
                .Count(sec => sec.Intent == SectionIntent.FamilyMembers);

            if (familySectionCount > 1)
            {
                return ApplicationTemplateDefinitionErrors.MultipleFamilySections;
            }

            // Replace existing steps with the new structure
            _Steps.Clear();
            _Steps.AddRange(newSteps);
            _fieldKeys = null; // invalidate cache

            // Recalculate capabilities based on the new steps
            RecalculateCapabilities();

            return Result.Success;
        }

        public Result<Success> SyncMembershipPlans(List<MembershipPlan> newPlans)
        {
            var newIds = newPlans.Select(t => t.Id).ToHashSet();

            // remove old ones
            _ConnectedMembershipPlans.RemoveAll(t => !newIds.Contains(t.Id));

            // add new ones
            foreach (var plan in newPlans)
            {
                var res = plan.AssignApplicationTemplate(this.Id,SupportsFamilyPlans);

                if (res.IsError) return res.TopError;
                _ConnectedMembershipPlans.Add(plan);
            }

            return Result.Success;
        }



        //ToSnapShot 
        public ApplicationTemplateSnapshot ToSnapshot(decimal BaseFee, List<PricingPolicySnapshot> policies,MembershipPlanSnapshot membershipPlan,
           List<InstallmentRuleSnapshot>installmentRules )
        {
 

            policies = policies.Where(p => p.Conditions.Any(c => FieldKeys.Contains(c.DependsOnFieldKey))).ToList();
            var snapshot = new ApplicationTemplateSnapshot(
                Id,
                Name,
                BaseFee,
                membershipPlan,
                policies, // Passed in from the Application Layer
                _Steps.OrderBy(s => s.Order).ToList(),
                installmentRules
            );

            var familySec = snapshot.Steps.SelectMany(s => s.Sections).FirstOrDefault(sec => sec.Intent == SectionIntent.FamilyMembers);
            if (familySec != null && membershipPlan.MaxFamilyMembers > 0)
            {
                var repeatRule = RepeatRule
                    .Create(membershipPlan.MaxFamilyMembers, RepeatMode.AtLeastOne)
                    .Value;

                familySec.SetRepeatRule(repeatRule);
            }

            return snapshot;
        }

        #region ensure invariants like no family fields if not supporting family plans, no system fields in general sections, etc.
        public Result<Success> ValidateConsistency()
        {
            RecalculateCapabilities();
            if (_ConnectedMembershipPlans.Any(p => p.MaxFamilyMembers > 0)
                && !SupportsFamilyPlans)
            {
                return Error.Conflict(
                    "Template.FamilySectionRequired",
                    "Template must include family section because it is used by family plans."
                );
            }

            
            return Result.Success;
        }
        
        private void RecalculateCapabilities()
        {
            SupportsFamilyPlans = _Steps
                .SelectMany(s => s.Sections)
                .Any(sec => sec.Intent == SectionIntent.FamilyMembers);
        }
        #endregion


        #region Assignment of Policies
        public Result<Success> AssignPolicy(PricingPolicy policy,int priority)
        {
            // Already assigned?
            if (_PricingPolicyAssignments.Any(a => a.PolicyId == policy.Id))
                return Error.Conflict("Template.PolicyAlreadyAssigned",
                    "This policy is already assigned to this template.");

            if(priority < 0 || priority > 100)
                return Error.Conflict("Template.InvalidPriority",
                    "Priority must be between 0 and 100.");

            if(_PricingPolicyAssignments.Any(a => a.Priority == priority))
                return Error.Conflict("Template.PriorityInUse",
                    "This priority is already in use.");

            var assignmentResult = PricingPolicyAssignment.Create(
                ClubId,
                policy.Id,
                Id,
                priority,
                PricingPolicyTargetType.ApplicationTemplate,
                policy,
                FieldKeys  // your existing HashSet<string>
            );

            if (assignmentResult.IsError) return assignmentResult.TopError;

            _PricingPolicyAssignments.Add(assignmentResult.Value);
            return Result.Success;
        }

        public Result<PricingPolicyAssignment> UnAssignPolicy(Guid policyId)
        {
            var assignment = _PricingPolicyAssignments
                .FirstOrDefault(a => a.PolicyId == policyId);

            if (assignment == null)
                return Error.NotFound("Template.PolicyAssignmentNotFound");

            _PricingPolicyAssignments.Remove(assignment);
            return assignment;

        }
        #endregion

    }

}
