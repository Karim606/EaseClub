using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace EaseClub.Domain.ApplicationTemplates.SystemSections
    {
        public class SystemSectionIntegrityComparer
        {
            public Result<Success> Validate(SectionIntent intent, List<FieldSpecification> incomingFields)
            {
                var registryShape = SystemSectionRegistry.Get(intent);
                var violations = new List<Error>();

                foreach (var systemField in registryShape.Fields)
                {
                    var incomingField = incomingFields.FirstOrDefault(f => f.Key == systemField.Key);

                    // 1. Mandatory Field Check
                    if (incomingField == null)
                    {
                        violations.Add(Error.Validation("Integrity.MissingField", $"System field '{systemField.Key}' is missing."));
                        continue;
                    }

                    if(systemField.Type != incomingField.Type) { violations.Add(Error.Validation("Integrity.IncompatibleFieldType",
                        $"System field '{systemField.Key}' has incompatible type.")); }

                    // 2. Compare Validation Rules
                    var ruleErrors = CompareRules(systemField, incomingField);
                    if (ruleErrors.Any()) violations.AddRange(ruleErrors);

                    // 3. Enum Allowed Values Check
                    if (systemField.Type == FieldType.Enum && incomingField.Type == FieldType.Enum)
                    {
                        if (incomingField.AllowedValues == null) { violations.Add(Error.Validation("Integrity.EnumValues",
                            $"Field '{systemField.Key}' is missing enum values.")); 
                        }

                        var extraValues = incomingField.AllowedValues?.Except(systemField.AllowedValues ?? new List<string>()).ToList();

                        if (extraValues != null && extraValues.Any())
                            violations.Add(Error.Validation(
                                "Integrity.EnumValues",
                                $"Field '{systemField.Key}' contains disallowed enum values: {string.Join(", ", extraValues)}"
                            ));
                        var missingValues = systemField.AllowedValues?.Except(incomingField.AllowedValues ?? new List<string>()).ToList();
                        if (missingValues != null && missingValues.Any())
                        {
                            violations.Add(Error.Validation(
                                "Integrity.EnumValues",
                                $"Field '{systemField.Key}' is missing enum values: {string.Join(", ", missingValues)}"
                            ));
                        }
                    }
                }

                if (violations.Any()) return violations;
                return Result.Success;
            }

            private List<Error> CompareRules(SystemFieldDefinition system, FieldSpecification incoming)
            {
                var errors = new List<Error>();

                if (incoming.ValidationRules == null)
                {
                    errors.Add(Error.Validation(
                        "Integrity.ValidationRulesMissing",
                        $"Field '{system.Key}' is missing validation rules."
                        ));
                    return errors;
                }

            void CheckRule<T>(T systemValue, T incomingValue, bool canOverride, string propName)
                {
                    if (Equals(systemValue, incomingValue)) return;

                // If they differ and the system doesn't allow overrides, fail
                    if (!canOverride)
                    errors.Add(Error.Validation("Integrity.RuleViolation", $"Field '{system.Key}' {propName} cannot be overridden."));
                }

                CheckRule(system.RuleSet.IsRequired, incoming.ValidationRules.IsRequired, system.ValidationRulesCanBeOverriden.IsRequired, "IsRequired");
                CheckRule(system.RuleSet.MinValue, incoming.ValidationRules.MinValue, system.ValidationRulesCanBeOverriden.MinValue , "MinValue");
                CheckRule(system.RuleSet.MaxValue, incoming.ValidationRules.MaxValue, system.ValidationRulesCanBeOverriden.MaxValue, "MaxValue");
                CheckRule(system.RuleSet.MinDate, incoming.ValidationRules.MinDate, system.ValidationRulesCanBeOverriden.MinDate, "MinDate");
                CheckRule(system.RuleSet.MaxDate, incoming.ValidationRules.MaxDate, system.ValidationRulesCanBeOverriden.MaxDate, "MaxDate");
                CheckRule(system.RuleSet.MinLength, incoming.ValidationRules.MinLength, system.ValidationRulesCanBeOverriden.MinLength, "MinLength");
                CheckRule(system.RuleSet.MaxLength, incoming.ValidationRules.MaxLength, system.ValidationRulesCanBeOverriden.MaxLength, "MaxLength");
                CheckRule(system.RuleSet.Regex, incoming.ValidationRules.Regex, system.ValidationRulesCanBeOverriden.Regex, "Regex");

                return errors;
            }
        }
    }

