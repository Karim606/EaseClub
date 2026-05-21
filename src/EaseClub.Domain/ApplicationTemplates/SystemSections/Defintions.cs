using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.SystemSections
{
    public record SystemSectionDefinition(
    SectionIntent Intent,
    string Title,
    IReadOnlyList<SystemFieldDefinition> Fields
    );

    public record SystemFieldDefinition(
    string Key,
    string Label,
    FieldType Type,
    ValidationRuleSet RuleSet,
    OverridenValidationRules ValidationRulesCanBeOverriden,
    List<string>?AllowedValues=null
    );

    public record FieldSpecification(
        string Key,
        FieldType Type,
        ValidationRuleSet ValidationRules,
        List<string>? AllowedValues = null
    );

    public record OverridenValidationRules(
            bool IsRequired = false,
            bool MinLength = false,
            bool MaxLength = false,
            bool Regex = false,
            bool MinValue = false,
            bool MaxValue = false,
            bool MinDate = false,
            bool MaxDate = false);


    public static class FamilyMemberField
    {
        public static string FullName = "sys_family_member_full_name";
        public static string DateOfBirth = "sys_family_member_dob";
        public static string Relationship = "sys_family_member_relationship";

        public static HashSet<string> All = new HashSet<string> { FullName, DateOfBirth, Relationship };
    }

}
