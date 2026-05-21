using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.ApplicationTemplates.SystemSections
{
    public static class SystemSectionRegistry
    {
        public static SystemSectionDefinition Get(SectionIntent intent)
        {
            return intent switch
            {
                SectionIntent.FamilyMembers => GetFamilyMembers(),
                _ => throw new ArgumentOutOfRangeException(nameof(intent))
            };
        }

        public static bool ResolveKeys(string key,SectionIntent intent)
        {
            return intent switch
            {

                SectionIntent.FamilyMembers => FamilyMemberField.All.Contains(key),
                _ => false
            };

        }
        private static SystemSectionDefinition GetFamilyMembers()
        {
            var nameRule = ValidationRuleSet.Create(
            isRequired: true,
            minLength: 2,
            maxLength: 100
            ).Value;

            var dobRule = ValidationRuleSet.Create(
                isRequired: true,
                minDate: new DateTime(1900, 1, 1),
                maxDate: DateTime.UtcNow
            ).Value;

            var relationRule = ValidationRuleSet.Create(
                isRequired: true
            ).Value;

            // shape of section 
            return new SystemSectionDefinition(
                SectionIntent.FamilyMembers,
                "Family Members",
                new List<SystemFieldDefinition>
                {
            new(
                Key: FamilyMemberField.FullName,
                Label: "Full Name",
                Type: FieldType.Text,
                RuleSet: nameRule,
                ValidationRulesCanBeOverriden:new OverridenValidationRules()
                ),

            new(
                Key: FamilyMemberField.DateOfBirth,
                Label: "Date Of Birth",
                Type: FieldType.Date,
                RuleSet: dobRule,
                ValidationRulesCanBeOverriden:new OverridenValidationRules(MinDate:true,MaxDate:true)
                ),

            new(
                Key: FamilyMemberField.Relationship,
                Label: "Relationship",
                Type: FieldType.Enum,
                RuleSet: relationRule,
                AllowedValues: new()
                {
                    "Father",
                    "Mother",
                    "Son",
                    "Daughter",
                    "Spouse"
                },
                ValidationRulesCanBeOverriden:new OverridenValidationRules()
                )
                }
            );
        }
    }
}


