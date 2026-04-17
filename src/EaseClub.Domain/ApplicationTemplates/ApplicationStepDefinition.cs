using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates
{
    public class ApplicationStepDefinition : AuditableEntity
    {
        // 1. EF Core Constructor
        private ApplicationStepDefinition() { }

        // 2. Private Constructor: Only the internal factory can call this
        private ApplicationStepDefinition(Guid id, Guid templateId, string title, int order)
            : base(id)
        {
            TemplateId = templateId;
            Title = title;
            Order = order;
        }

        public Guid TemplateId { get; private set; }
        public ApplicationTemplateDefinition Template {  get; private set; }
        //public string Category { get; private set; } // e.g., "IDENTITY", "DOCUMENTS"
        public string Title { get; private set; }
        public int Order { get; internal set; }

        private readonly List<ApplicationSectionDefinition> _Sections = new();
        public IReadOnlyList<ApplicationSectionDefinition> Sections => _Sections.AsReadOnly();

        // 3. Internal Factory: Only ApplicationTemplateDefinition can call this
        internal static Result<ApplicationStepDefinition> Create(
            Guid id,
            Guid templateId,
            string title,
            int order)
        {
            if (templateId == Guid.Empty) return ApplicationStepErrors.TemplateIdRequired;
            if (string.IsNullOrEmpty(title)) return ApplicationStepErrors.TitleRequired;

            return new ApplicationStepDefinition(id, templateId, title, order);
        }

        public Result<Success>Update( string title)
        {
            if (string.IsNullOrEmpty(title)) return ApplicationStepErrors.TitleRequired;

            Title = title;
            return Result.Success;
        }
        public void UpdateOrder(int order)
        {
            Order = order;
        }

        // 4. Factory Method for Child (Section)
        public Result<ApplicationSectionDefinition> AddNewSection(string title, RepeatRule? repeatRule = null,
            SectionIntent intent = SectionIntent.General)
        {
            // Business Rule: Ensure section title isn't duplicated within this specific step
            if (_Sections.Any(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                return ApplicationStepErrors.DuplicateSectionTitle;

            if (_Sections.Any(s => s.Intent == SectionIntent.FamilyMembers)&& intent == SectionIntent.FamilyMembers)
                return Error.Conflict("Can't add more than one family member section.");

            var sectionResult = ApplicationSectionDefinition.Create(
                Guid.NewGuid(),
                this.Id,
                title,
                _Sections.Count+1,
                repeatRule,
                intent
            );

            if (sectionResult.IsError) return sectionResult.TopError;

            _Sections.Add(sectionResult.Value);
     
            return sectionResult.Value;
        }

        public Result<Success> RemoveSection(Guid sectionId)
        {
            var existingSection = _Sections.FirstOrDefault(s => s.Id == sectionId);

            if (existingSection == null)
                return ApplicationStepErrors.SectionDoesntExist;
            
            var removedOrder = existingSection.Order;
            _Sections.Remove(existingSection);

            return Result.Success;
        }

        public Result<Success> ReorderSections(List<Guid> sectionIdsInOrder)
        {
            for (int i = 0; i < sectionIdsInOrder.Count; i++)
            {
                var section = _Sections.FirstOrDefault(s => s.Id == sectionIdsInOrder[i]);
                if (section == null) return ApplicationStepErrors.SectionDoesntExist;
                section.UpdateOrder(i + 1);
            }
            return Result.Success;
        }

        //ToSnapshot
        public StepSnapshot ToSnapshot()
        {
            return new StepSnapshot(
                Id,
                Title,
                Order,
                _Sections.OrderBy(s => s.Order).Select(s => s.ToSnapshot()).ToList()
            );
        }
    }
}