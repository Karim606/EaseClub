using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpsertTemplate;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EaseClub.Application.Features.ApplicationTemplates.Services
{
    // ============================================================
    // STEP 2: TemplateUpdater — only applies diffs, zero logic
    // ============================================================

    public class TemplateUpdater
    {
        private readonly ApplicationTemplateDefinition _template;

        public TemplateUpdater(ApplicationTemplateDefinition template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));
        }

        public Result<Success> ApplySteps(List<StepDetailsDto> incomingSteps)
        {
            var diff = TemplateDiff.Compute(_template.Steps, incomingSteps);

            // 1. Remove
            foreach (var step in diff.Removed)
            {
                var result = _template.RemoveStep(step.Id);
                if (result.IsError) return result.TopError;
            }

            // 2. Add
            foreach (var stepDto in diff.Added)
            {
                var result = _template.AddNewStep(stepDto.Title, order: 0);
                if (result.IsError) return result.TopError;

                var step = result.Value;
                var sectionsResult = ApplySections(step, stepDto.Sections);
                if (sectionsResult.IsError) return sectionsResult.TopError;
            }

            // 3. Update
            foreach (var (existing, incoming) in diff.Updated)
            {
                var result = existing.Update(incoming.Title);
                if (result.IsError) return result.TopError;

                var sectionsResult = ApplySections(existing, incoming.Sections);
                if (sectionsResult.IsError) return sectionsResult.TopError;
            }

            // 4. Reorder once at the end — domain owns order
            return _template.ReorderSteps(incomingSteps.Select(s => s.Id).ToList());
        }

        // --------------------------------------------------------
        private Result<Success> ApplySections(
            ApplicationStepDefinition step,
            List<SectionDetailsDto> incomingSections)
        {
            var diff = SectionDiff.Compute(step.Sections, incomingSections);

            // 1. Remove
            foreach (var section in diff.Removed)
            {
                var result = step.RemoveSection(section.Id);
                if (result.IsError) return result.TopError;
            }

            // 2. Add
            foreach (var secDto in diff.Added)
            {
                var repeatRule = ResolveRepeatRule(secDto.RepeatRule);
                if (repeatRule.IsError) return repeatRule.TopError;

                if (secDto.Intent != SectionIntent.General)
                {
                    var integrity = ValidateSystemSection(secDto);
                    if (integrity.IsError) return integrity.TopError;
                }

                var result = step.AddNewSection(secDto.Title, repeatRule.Value, secDto.Intent);
                if (result.IsError) return result.TopError;

                var fieldsResult = ApplyFields(result.Value, secDto.Fields);
                if (fieldsResult.IsError) return fieldsResult.TopError;
            }

            // 3. Update
            foreach (var (existing, incoming) in diff.Updated)
            {
                var repeatRule = ResolveRepeatRule(incoming.RepeatRule);
                if (repeatRule.IsError) return repeatRule.TopError;

                if (incoming.Intent != SectionIntent.General)
                {
                    var integrity = ValidateSystemSection(incoming);
                    if (integrity.IsError) return integrity.TopError;
                }

                var result = existing.Update(incoming.Title, repeatRule.Value);
                if (result.IsError) return result.TopError;

                var fieldsResult = ApplyFields(existing, incoming.Fields);
                if (fieldsResult.IsError) return fieldsResult.TopError;
            }

            // 4. Reorder once at the end
            return step.ReorderSections(incomingSections.Select(s => s.Id).ToList());
        }

        // --------------------------------------------------------
        private Result<Success> ApplyFields(
            ApplicationSectionDefinition section,
            List<FieldDetailsDto> incomingFields)
        {
            var diff = FieldDiff.Compute(section.Fields, incomingFields);

            // 1. Remove
            foreach (var field in diff.Removed)
            {
                var result = _template.RemoveField(section.Id, field.Id);
                if (result.IsError) return result.TopError;
            }

            // 2. Add
            foreach (var fieldDto in diff.Added)
            {
                var validationResult = fieldDto.ValidationRules.ToDomain();
                if (validationResult.IsError) return validationResult.TopError;

                var result = _template.AddFieldToSection(
                    fieldDto.Id,
                    section.Id,
                    string.IsNullOrWhiteSpace(fieldDto.Key) ? null : fieldDto.Key,
                    fieldDto.Label,
                    fieldDto.FieldType,
                    validationResult.Value,
                    visibilityCondition: null,
                    persistToMembership: false,
                    fieldDto.AllowedValues
                );
                if (result.IsError) return result.TopError;
            }

            // 3. Update
            foreach (var (existing, incoming) in diff.Updated)
            {
                var validationResult = incoming.ValidationRules.ToDomain();
                if (validationResult.IsError) return validationResult.TopError;

                var result = existing.Update(
                    incoming.Label,
                    validationResult.Value,
                    visibilityCondition: null,
                    existing.PersistToMembership,
                    existing.Type == FieldType.Enum ? incoming.AllowedValues : null
                );
                if (result.IsError) return result.TopError;
            }

            // 4. Reorder once at the end
            return section.ReorderFields(incomingFields.Select(f => f.Id).ToList());
        }


        // ============================================================
        // Private Helpers — small, focused, reusable
        // ============================================================

        private Result<RepeatRule?> ResolveRepeatRule(RepeatRuleSetDto? dto)
        {
            if (dto == null) return null;

            var result = RepeatRule.Create(dto.NumberOfRepeats, dto.Mode);
            if (result.IsError) return result.TopError;

            return(result.Value);
        }

        private Result<Success> ValidateSystemSection(SectionDetailsDto secDto)
        {
            var comparer = new SystemSectionIntegrityComparer();
            return comparer.Validate(
                secDto.Intent,
                secDto.Fields.Select(f => f.ToFieldSpecification()).ToList());
        }
    }
}
