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

        //private readonly List<ApplicationFieldDefinition> _Fields = new();
        //public  IReadOnlyList<ApplicationFieldDefinition> Fields => _Fields.AsReadOnly();

        private readonly List<ApplicationStepDefinition> _Steps = new();
        public IReadOnlyList<ApplicationStepDefinition> Steps => _Steps.AsReadOnly();

        private readonly List<MembershipPlan> _ConnectedMembershipPlans = new();
        public IReadOnlyList<MembershipPlan> ConnectedMembershipPlans => _ConnectedMembershipPlans.AsReadOnly();

        private readonly List<PricingPolicyAssignment> _PricingPolicyAssignments = new();
        public IReadOnlyList<PricingPolicyAssignment> PricingPolicyAssignments  => _PricingPolicyAssignments.AsReadOnly();

        private HashSet<string>? _fieldKeys;

        private HashSet<string> FieldKeys
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

        #region Step Management through Template (to enforce invariants like unique titles and order)
        // 4. THE AGGREGATE GATEKEEPER: Create Step through Template
        public Result<ApplicationStepDefinition> AddNewStep(string category, string title, int order)
        {
            if (_Steps.Any(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                return ApplicationTemplateDefinitionErrors.DuplicateStepTitle;

            if (order < 0 || order > _Steps.Count) return ApplicationTemplateDefinitionErrors.InvalidStepOrder;

            var stepResult = ApplicationStepDefinition.Create(
                Guid.NewGuid(),
                this.Id,
                category,
                title,
                order
            );

            if (stepResult.IsError) return stepResult.TopError;

            _Steps.Add(stepResult.Value);

            return stepResult.Value;
        }

        public Result<Success> RemoveStep(Guid stepId)
        {
            var step = _Steps.FirstOrDefault(s => s.Id == stepId);

            if (step != null)
            {
                _Steps.Remove(step);
            }

            return Result.Success;
        }
        #endregion 

        #region Field Management through Template (to enforce invariants like unique keys)
        public Result<ApplicationFieldDefinition> AddFieldToSection(
            Guid id,
            Guid sectionId,
            string? key,
            string label,
            FieldType type,
            ValidationRuleSet rules,
            ConditionExpression? visibilityCondition,
            bool persistToMembership,
            List<string>? allowedValues = null)
        {
            var section = FindSection(sectionId);

            if (section == null)
                return Error.NotFound("Template.SectionNotFound");

            var finalKey = ResolveKey(key, label);

            var isSystemField = section.Intent != SectionIntent.General && SystemSectionRegistry.ResolveKeys(finalKey, section.Intent);
            // enforce uniqueness
            if (!FieldKeys.Add(finalKey))
                return Error.Conflict("Template.DuplicateKey",
                    $"Field key '{finalKey}' already exists.");

            var fieldResult = section.CreateField(
                id,
                Id,
                finalKey,
                label,
                type,
                rules,
                visibilityCondition,
                persistToMembership,
                isSystemField,
                allowedValues
            );

            if (fieldResult.IsError)
            {
                FieldKeys.Remove(finalKey);
                return fieldResult.TopError;
            }

            return fieldResult.Value;
        }


        public Result<Success> RemoveField(Guid sectionId, Guid fieldId)
        {
            // 1. Find the section
            var section = FindSection(sectionId);

            if (section == null) return Error.NotFound("Template.SectionNotFound");

            // 2. Find the field to check its Key
            var field = section.Fields.FirstOrDefault(f => f.Id == fieldId);
            if (field == null) return Error.NotFound("Template.FieldNotFound");

            if (field.IsSystemField)
            {
                return Error.Validation("Template.FieldLocked",
                    $"Field '{field.Key}' is a system-required field and cannot be removed.");
            }

            // 4. If safe, tell the section to remove it
            var res =  section.RemoveField(fieldId);

            if(res.IsError) return res.TopError;

            FieldKeys.Remove(field.Key);

            return Result.Success;
        }

        public Result<Success> ReorderSteps(List<Guid> stepIdsInOrder)
        {
            for (int i = 0; i < stepIdsInOrder.Count; i++)
            {
                var step = _Steps.FirstOrDefault(s => s.Id == stepIdsInOrder[i]);
                if (step == null) return ApplicationTemplateDefinitionErrors.StepDoesntExist;
                step.UpdateOrder(i + 1);
            }
            return Result.Success;
        }

        //GenerateUniqueKey
        private string GenerateUniqueKey(string label)
        {
            // 1. Basic Slugify: "Full Name!" -> "full_name"
            var baseKey = new string(label.ToLower().Trim()
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray())
                .Replace("__", "_");

            var existingKeys = _Steps
                .SelectMany(s => s.Sections)
                .SelectMany(sec => sec.Fields)
                .Select(f => f.Key)
                .ToHashSet();

            // 2. Collision Loop: if "full_name" exists, try "full_name_1", etc.
            var uniqueKey = baseKey;
            int counter = 1;
            while (existingKeys.Contains(uniqueKey))
            {
                uniqueKey = $"{baseKey}_{counter++}";
            }

            return uniqueKey;
        }

        private string ResolveKey(string? key, string label)
        {
            if (!string.IsNullOrWhiteSpace(key))
                return key.ToLower().Trim();

            return GenerateUniqueKey(label);
        }

        #endregion

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
                _Steps.OrderBy(s => s.Order).Select(s => s.ToSnapshot()).ToList(),
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


        private ApplicationSectionDefinition? FindSection(Guid sectionId)
        {
            return _Steps
                .SelectMany(s => s.Sections)
                .FirstOrDefault(s => s.Id == sectionId);
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

            var assignmentResult = PricingPolicyAssignment.Create(
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

        public Result<Success> UnassignPolicy(Guid policyId)
        {
            var assignment = _PricingPolicyAssignments
                .FirstOrDefault(a => a.PolicyId == policyId);

            if (assignment == null)
                return Error.NotFound("Template.PolicyAssignmentNotFound");

            _PricingPolicyAssignments.Remove(assignment);
            return Result.Success;
        }
        #endregion

    }

}
