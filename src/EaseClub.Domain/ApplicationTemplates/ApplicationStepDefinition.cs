using EaseClub.Domain.ApplicationTemplates.Errors;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
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
        private ApplicationStepDefinition(Guid id, Guid templateId, string category, string title, int order)
            : base(id)
        {
            TemplateId = templateId;
            Category = category;
            Title = title;
            Order = order;
        }

        public Guid TemplateId { get; private set; }
        public ApplicationTemplateDefinition Template {  get; private set; }
        public string Category { get; private set; } // e.g., "IDENTITY", "DOCUMENTS"
        public string Title { get; private set; }
        public int Order { get; internal set; }

        private readonly List<ApplicationSectionDefinition> _Sections = new();
        public IReadOnlyList<ApplicationSectionDefinition> Sections => _Sections.AsReadOnly();

        // 3. Internal Factory: Only ApplicationTemplateDefinition can call this
        internal static Result<ApplicationStepDefinition> Create(
            Guid id,
            Guid templateId,
            string category,
            string title,
            int order)
        {
            if (templateId == Guid.Empty) return ApplicationStepErrors.TemplateIdRequired;
            if (string.IsNullOrEmpty(title)) return ApplicationStepErrors.TitleRequired;
            if (string.IsNullOrEmpty(category)) return ApplicationStepErrors.CategoryRequired;

            return new ApplicationStepDefinition(id, templateId, category, title, order);
        }

        public Result<Success>Update(string category, string title)
        {
            if (string.IsNullOrEmpty(title)) return ApplicationStepErrors.TitleRequired;
            if (string.IsNullOrEmpty(category)) return ApplicationStepErrors.CategoryRequired;

            Category = category;
            Title = title;
            return Result.Success;
        }
        internal void UpdateOrder(int order)
        {
            Order = order;
        }

        // 4. Factory Method for Child (Section)
        public Result<ApplicationSectionDefinition> AddNewSection(string title, int order, RepeatRule? repeatRule = null)
        {
            // Business Rule: Ensure section title isn't duplicated within this specific step
            if (_Sections.Any(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                return ApplicationStepErrors.DuplicateSectionTitle;

            if (order < 0 || order > _Sections.Count) return ApplicationStepErrors.InvalidSectionOrder;

            var sectionResult = ApplicationSectionDefinition.Create(
                Guid.NewGuid(),
                this.Id,
                title,
                order,
                repeatRule
            );

            if (sectionResult.IsError) return sectionResult.TopError;

            // 3. SHIFTING LOGIC: Move existing sections forward
            foreach (var existingSection in _Sections.Where(s => s.Order >= order))
            {
                existingSection.UpdateOrder(existingSection.Order + 1);
            }

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

            foreach (var remainingSection in _Sections.Where(s => s.Order > removedOrder))
            {
                remainingSection.UpdateOrder(remainingSection.Order - 1);
            }
            return Result.Success;
        }
    }
}