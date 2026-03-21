using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.MembershipApplications
{
    public static class ApplicationTestDataBuilder
    {
        private static MembershipPlanSnapshot DefaultPlan =>
            new(Guid.NewGuid(), "Standard Plan", 30, 100, 1, 5);

        public static List<StepSnapshot> CreateSteps(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => CreateStep(i))
                .ToList();
        }

        public static ApplicationTemplateSnapshot CreateSnapshot(int stepCount, List<StepSnapshot>? customSteps = null, decimal baseFee = 100)
        {
            var steps = customSteps ?? Enumerable.Range(1, stepCount)
                .Select(i => CreateStep(i)).ToList();

            return new ApplicationTemplateSnapshot(
                Guid.NewGuid(), "Template", baseFee, DefaultPlan, new(), steps, new());
        }

        public static StepSnapshot CreateStep(int order, List<SectionSnapshot>? sections = null)
            => new(Guid.NewGuid(), $"Step {order}", order, sections ?? new());

        public static StepSnapshot CreateStepWithSection(int order, List<SectionSnapshot> sections)
            => new(Guid.NewGuid(), "Title", order, sections);

        public static SectionSnapshot CreateSection(string title, int order, RepeatRule? rule, List<FieldSnapshot> fields)
            => new(Guid.NewGuid(), title, order, rule, SectionIntent.General, fields);

        public static FieldSnapshot CreateField(Guid id, string key, FieldType type)
            => new(id, key, "Label", type, ValidationRuleSet.Create(false).Value.ToSnapshot(), null, 0, false);
    }
}
