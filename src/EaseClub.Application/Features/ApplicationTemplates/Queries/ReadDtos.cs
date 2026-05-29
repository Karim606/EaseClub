using EaseClub.Application.Common.Dtos;
using EaseClub.Application.Features.ApplicationTemplates.Commands;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.RepeatRule;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries
{
    public record TemplateSummaryDto(Guid Id, string Name,bool IsActive, DateTime CreatedAt, DateTime? LastModified, List<string> ConnectedMembershipPlans, bool SupportsFamilyPlans);

    public record TemplateTreeQuery(
    Guid Id,
    string Name,
    bool SupportsFamilyPlans,
    bool HasConnectedFamilyPlan,
    string? ConnectedFamilyPlanName,
    int? MaxFamilyMembersOfConnectedPlan,
    List<StepQuery> Steps
);

    public record StepQuery(
        Guid Id,
        string Title,
        int Order,
        List<SectionQuery> Sections
    );

    public record SectionQuery(
        Guid Id,
        string Title,
        int Order,
        SectionIntent Intent,
        RepeatRule? RepeatRule,
        List<FieldQuery> Fields

    );

    public record FieldQuery(
        Guid Id,
        string Key,
        string Label,
        FieldType FieldType,
        ValidationRuleSet ValidationRules,
        List<string>? AllowedValues,
        bool IsSystemField  // Helpful flag for the UI to handle logic
    );

}
