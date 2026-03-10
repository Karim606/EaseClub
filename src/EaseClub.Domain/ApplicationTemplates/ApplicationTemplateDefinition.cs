using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ConditionExpression;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        //private readonly List<ApplicationFieldDefinition> _Fields = new();
        //public  IReadOnlyList<ApplicationFieldDefinition> Fields => _Fields.AsReadOnly();

        private readonly List<ApplicationStepDefinition> _Steps = new();
        public IReadOnlyList<ApplicationStepDefinition> Steps => _Steps.AsReadOnly();

        private readonly List<MembershipType> _ConnectedMembershipTypes = new();
        public IReadOnlyList<MembershipType> ConnectedMembershipTypes => _ConnectedMembershipTypes.AsReadOnly();

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

            // 3. SHIFTING LOGIC: Move existing steps forward
            foreach (var existingStep in _Steps.Where(s => s.Order >= order))
            {
                existingStep.UpdateOrder(existingStep.Order + 1);
            }

            _Steps.Add(stepResult.Value);

            return stepResult.Value;
        }

        public Result<Success> RemoveStep(Guid stepId)
        {
            var existingStep = _Steps.FirstOrDefault(s => s.Id == stepId);

            if (existingStep == null)
                return ApplicationTemplateDefinitionErrors.StepDoesntExist;

            int removedOrder = existingStep.Order;

            _Steps.Remove(existingStep);

            // 3. SHIFTING LOGIC: Close the gap
            foreach (var remainingStep in _Steps.Where(s => s.Order > removedOrder))
            {
                remainingStep.UpdateOrder(remainingStep.Order - 1);
            }

            return Result.Success;
        }

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

            // 4. If safe, tell the section to remove it
            return section.RemoveField(fieldId);
        }
        public Result<Success> ReorderFieldsInSection(Guid sectionId, List<Guid> newOrderIds)
        {
            // 1. Find the section
            var section = _Steps.SelectMany(s => s.Sections)
                                .FirstOrDefault(s => s.Id == sectionId);

            if (section == null) return Error.NotFound("Template.SectionNotFound");

            // 2. Delegate the physical reordering to the section
            return section.ReorderFields(newOrderIds);
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

        public Result<Success> SyncMembershipTypes(List<MembershipType> newTypes)
        {
            var newIds = newTypes.Select(t => t.Id).ToHashSet();

            // remove old ones
            _ConnectedMembershipTypes.RemoveAll(t => !newIds.Contains(t.Id));

            // add new ones
            foreach (var type in newTypes)
            {
                if (_ConnectedMembershipTypes.All(t => t.Id != type.Id))
                {
                    _ConnectedMembershipTypes.Add(type);
                }
            }

            return Result.Success;
        }

        //ToSnapShot 
        public ApplicationTemplateSnapshot ToSnapshot(decimal BaseFee, List<PricingPolicySnapshot> policies,MembershipPlanSnapshot membershipPlan,
           List<InstallmentRuleSnapshot>installmentRules )
        {
             policies = policies.Where(p => p.Conditions.Any(c => FieldKeys.Contains(c.DependsOnFieldKey))).ToList();
            return new ApplicationTemplateSnapshot(
                Id,
                Name,
                BaseFee,
                membershipPlan,
                policies, // Passed in from the Application Layer
                _Steps.OrderBy(s => s.Order).Select(s => s.ToSnapshot()).ToList(),
                installmentRules
            );
        }


        private ApplicationSectionDefinition? FindSection(Guid sectionId)
        {
            return _Steps
                .SelectMany(s => s.Sections)
                .FirstOrDefault(s => s.Id == sectionId);
        }

        private string ResolveKey(string? key, string label)
        {
            if (!string.IsNullOrWhiteSpace(key))
                return key.ToLower().Trim();

            return GenerateUniqueKey(label);
        }
    }
}
