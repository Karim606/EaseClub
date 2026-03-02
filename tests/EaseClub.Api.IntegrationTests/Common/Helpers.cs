using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Api.IntegrationTests.Common
{
    public  class Helpers
    {
        private readonly AppDbContext _context;
        public Helpers(AppDbContext context)
        {
            _context = context;
        }

        public  async Task<(Guid templateId, Guid stepId, Guid sectionId, Guid fieldId)> 
            CreateFullTemplateHierarchyAsync(Guid clubId)
        {
            // 1. Create Template
            var template = ApplicationTemplateDefinition.Create(Guid.NewGuid(), clubId, "Full Template").Value;
            await _context.ApplicationTemplateDefinitions.AddAsync(template);

            // 2. Add Step
            var step = template.AddNewStep("General", "Step 1", 0).Value;
            await _context.ApplicationStepDefinitions.AddAsync(step);

            // 3. Add Section
            var section = step.AddNewSection("Details", 0).Value;
            await _context.ApplicationSectionDefinitions.AddAsync(section);

            // 4. Add Field
            var rules = ValidationRuleSet.Create(true).Value;
            var field = template.AddFieldToSection(Guid.NewGuid(), section.Id, "full_name", "Full Name",
                FieldType.Text, rules, null, false).Value;
            await _context.ApplicationFieldDefinitions.AddAsync(field);

            await _context.SaveChangesAsync();

            return (template.Id, step.Id, section.Id, field.Id);
        }

        public async Task<Guid> AddStepAsync(Guid templateId, string title, int order)
        {
            var template = await _context.ApplicationTemplateDefinitions
                .Include(t => t.Steps)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) throw new Exception("Template not found");

            var step = template.AddNewStep("General", title, order).Value;
            await _context.ApplicationStepDefinitions.AddAsync(step);
            await _context.SaveChangesAsync();

            return step.Id;
        }

        public async Task<Guid> AddSectionAsync(Guid stepId, string title, int order)
        {
            var step = await _context.ApplicationStepDefinitions
                .Include(s => s.Sections)
                .FirstOrDefaultAsync(s => s.Id == stepId);

            if (step == null) throw new Exception("Step not found");

            var section = step.AddNewSection(title, order).Value;
            await _context.ApplicationSectionDefinitions.AddAsync(section);
            await _context.SaveChangesAsync();

            return section.Id;
        }

        public async Task<Guid> AddFieldAsync(Guid templateId, Guid sectionId, string key, string label)
        {
            var template = await _context.ApplicationTemplateDefinitions
                .Include(t => t.Steps)
                    .ThenInclude(s => s.Sections)
                        .ThenInclude(sec => sec.Fields)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) throw new Exception("Template not found");

            var rules = ValidationRuleSet.Create(false).Value; // Non-required for simple tests

            var field = template.AddFieldToSection(
                Guid.NewGuid(),
                sectionId,
                key,
                label,
                FieldType.Text,
                rules,
                null,
                false).Value;

            await _context.ApplicationFieldDefinitions.AddAsync(field);
            await _context.SaveChangesAsync();

            return field.Id;
        }
    }
}
