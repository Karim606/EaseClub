using EaseClub.Domain.ApplicationTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Commands.Template.UpsertTemplate
{
    public class TemplateDiff
    {
        public List<StepDetailsDto> Added { get; init; } = new();
        public List<ApplicationStepDefinition> Removed { get; init; } = new();
        public List<(ApplicationStepDefinition Existing, StepDetailsDto Incoming)> Updated { get; init; } = new();

        /// <summary>
        /// Pure computation — no domain calls, no side effects.
        /// Just compares two sets and buckets the differences.
        /// </summary>
        public static TemplateDiff Compute(
            IReadOnlyList<ApplicationStepDefinition> existing,
            List<StepDetailsDto> incoming)
        {
            var existingMap = existing.ToDictionary(s => s.Id);
            var incomingMap = incoming.ToDictionary(s => s.Id);

            var added = incoming
                .Where(s => !existingMap.ContainsKey(s.Id))
                .ToList();

            var removed = existing
                .Where(s => !incomingMap.ContainsKey(s.Id))
                .ToList();

            var updated = incoming
                .Where(s => existingMap.ContainsKey(s.Id))
                .Select(s => (existingMap[s.Id], s))
                .ToList();

            return new TemplateDiff
            {
                Added = added,
                Removed = removed,
                Updated = updated
            };
        }
    }

    public class SectionDiff
    {
        public List<SectionDetailsDto> Added { get; init; } = new();
        public List<ApplicationSectionDefinition> Removed { get; init; } = new();
        public List<(ApplicationSectionDefinition Existing, SectionDetailsDto Incoming)> Updated { get; init; } = new();

        public static SectionDiff Compute(
            IReadOnlyList<ApplicationSectionDefinition> existing,
            List<SectionDetailsDto> incoming)
        {
            var existingMap = existing.ToDictionary(s => s.Id);
            var incomingMap = incoming.ToDictionary(s => s.Id);

            return new SectionDiff
            {
                Added = incoming.Where(s => !existingMap.ContainsKey(s.Id)).ToList(),
                Removed = existing.Where(s => !incomingMap.ContainsKey(s.Id)).ToList(),
                Updated = incoming
                    .Where(s => existingMap.ContainsKey(s.Id))
                    .Select(s => (existingMap[s.Id], s))
                    .ToList()
            };
        }
    }

    public class FieldDiff
    {
        public List<FieldDetailsDto> Added { get; init; } = new();
        public List<ApplicationFieldDefinition> Removed { get; init; } = new();
        public List<(ApplicationFieldDefinition Existing, FieldDetailsDto Incoming)> Updated { get; init; } = new();

        public static FieldDiff Compute(
            IReadOnlyList<ApplicationFieldDefinition> existing,
            List<FieldDetailsDto> incoming)
        {
            var existingMap = existing.ToDictionary(f => f.Id);
            var incomingMap = incoming.ToDictionary(f => f.Id);

            return new FieldDiff
            {
                Added = incoming.Where(f => !existingMap.ContainsKey(f.Id)).ToList(),
                Removed = existing.Where(f => !incomingMap.ContainsKey(f.Id)).ToList(),
                Updated = incoming
                    .Where(f => existingMap.ContainsKey(f.Id))
                    .Select(f => (existingMap[f.Id], f))
                    .ToList()
            };
        }
    }

}
