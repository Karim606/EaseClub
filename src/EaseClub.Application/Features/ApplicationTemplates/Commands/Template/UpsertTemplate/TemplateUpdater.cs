using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EaseClub.Application.Features.ApplicationTemplates.Services
{
    public class TemplateUpdater
    {
        private readonly ApplicationTemplateDefinition _template;

        public TemplateUpdater(ApplicationTemplateDefinition template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Upsert all steps, sections, and fields
        /// </summary>
        public Result<Success> UpsertSteps(List<StepDetailsDto> stepDtos)
        {
            var existingSteps = _template.Steps.ToDictionary(s => s.Id, s => s);

            foreach (var stepDto in stepDtos.OrderBy(s => s.Order))
            {
                var stepResult = UpsertStep(stepDto, existingSteps);
                if (stepResult.IsError) return stepResult.TopError;
            }

            // Remove steps not in DTO
            foreach (var removedStep in existingSteps.Values)
            {
                var removeResult = _template.RemoveStep(removedStep.Id);
                if (removeResult.IsError) return removeResult.TopError;
            }

            return Result.Success;
        }

        // -------------------------------
        // Step Level
        // -------------------------------
        private Result<Success> UpsertStep(StepDetailsDto stepDto, Dictionary<Guid, ApplicationStepDefinition> existingSteps)
        {
            ApplicationStepDefinition step;

            if (existingSteps.TryGetValue(stepDto.Id, out step))
            {
                var updateResult = step.Update(stepDto.Title);
                if (updateResult.IsError) return updateResult.TopError;

                existingSteps.Remove(stepDto.Id);
            }
            else
            {
                var stepResult = _template.AddNewStep(stepDto.Category, stepDto.Title, stepDto.Order);
                if (stepResult.IsError) return stepResult.TopError;
                step = stepResult.Value;
            }

            // Upsert sections inside this step
            return UpsertSections(step, stepDto.Sections);
        }

        // -------------------------------
        // Section Level
        // -------------------------------
        private Result<Success> UpsertSections(ApplicationStepDefinition step, List<SectionDetailsDto> sectionDtos)
        {
            var existingSections = step.Sections.ToDictionary(s => s.Id, s => s);

            foreach (var secDto in sectionDtos.OrderBy(s => s.Order))
            {
                var secResult = UpsertSection(step, secDto, existingSections);
                if (secResult.IsError) return secResult.TopError;
            }

            // Remove sections not in DTO
            foreach (var removedSection in existingSections.Values)
            {
                var removeResult = step.RemoveSection(removedSection.Id);
                if (removeResult.IsError) return removeResult.TopError;
            }

            return Result.Success;
        }

        private Result<Success> UpsertSection(ApplicationStepDefinition step, SectionDetailsDto secDto, Dictionary<Guid, 
            ApplicationSectionDefinition> existingSections)
        {
            ApplicationSectionDefinition section;
            RepeatRule? repeatRule = null;

            if(secDto.RepeatRule!=null) repeatRule = secDto.RepeatRule.ToDomain().IsSuccess ? secDto.RepeatRule.ToDomain().Value : null;

            if (secDto.Intent != SectionIntent.General)
            {
                var comparer = new SystemSectionIntegrityComparer();
                // We pass the section's fields to verify against the registry
                var integrityResult = comparer.Validate(secDto.Intent, secDto.Fields.Select(f=> f.ToFieldSpecification()).ToList());
                if (integrityResult.IsError) return integrityResult;
            }

            if (existingSections.TryGetValue(secDto.Id, out section))
            {
                if(repeatRule == null) repeatRule = section.RepeatRule;
                var updateResult = section.Update(secDto.Title,repeatRule);
                if (updateResult.IsError) return updateResult.TopError;

                existingSections.Remove(secDto.Id);
            }
            else
            {
                var secResult = step.AddNewSection(secDto.Title, secDto.Order, repeatRule, secDto.Intent);
                if (secResult.IsError) return secResult.TopError;
                section = secResult.Value;
            }

            // Upsert fields via Template (enforces key uniqueness)
            return UpsertFields(section, secDto.Fields);
        }

        // -------------------------------
        // Field Level
        // -------------------------------
        private Result<Success> UpsertFields(ApplicationSectionDefinition section, List<FieldDetailsDto> fieldDtos)
        {
            var existingFields = section.Fields.ToDictionary(f => f.Id, f => f);

            foreach (var fieldDto in fieldDtos)
            {
                var fieldResult = UpsertField(section, fieldDto, existingFields);
                if (fieldResult.IsError) return fieldResult.TopError;
            }

            // Remove fields not in DTO via Template
            foreach (var removedField in existingFields.Values)
            {
                var removeResult = _template.RemoveField(section.Id, removedField.Id);
                if (removeResult.IsError) return removeResult.TopError;
            }

            return Result.Success;
        }

        private Result<Success> UpsertField(ApplicationSectionDefinition section, FieldDetailsDto fieldDto, Dictionary<Guid, ApplicationFieldDefinition> existingFields)
        {
            var validationRules = fieldDto.ValidationRules.ToDomain();
            if(validationRules.IsError) return validationRules.TopError;

            if (existingFields.TryGetValue(fieldDto.Id, out var field))
            {
                var updateResult = field.Update(
                    fieldDto.Label,
                    validationRules.Value,
                    null,
                    field.PersistToMembership,
                    field.Type == FieldType.Enum ? field.AllowedValues : null
                );
                if (updateResult.IsError) return updateResult.TopError;

                existingFields.Remove(fieldDto.Id);
            }
            else
            {
                // Add new field via Template (handles unique keys)
                var addResult = _template.AddFieldToSection(
                    fieldDto.Id,
                    section.Id,
                    string.IsNullOrWhiteSpace(fieldDto.Key) ? null : fieldDto.Key,
                    fieldDto.Label,
                    fieldDto.FieldType,
                    validationRules.Value,
                    null,
                    persistToMembership: false,
                    fieldDto.AllowedValues
                );

                if (addResult.IsError) return addResult.TopError;
            }

            return Result.Success;
        }
    }
}
