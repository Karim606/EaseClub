using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications
{
    public class MembershipApplication : AuditableEntity
    {
        private MembershipApplication() { }

        private MembershipApplication(
            Guid id,
            string trackingNumber,
            ApplicationTemplateSnapshot templateSnapshot,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid templateId
           )
            : base(id)
        {
            TrackingNumber = trackingNumber;
            TemplateSnapshot = templateSnapshot;
            UserId = userId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            TemplateId = templateId;
            Status = ApplicationStatus.Draft;
            PricingState = PricingState.Estimated;
        }
        public string TrackingNumber { get; private set; }
        public ApplicationTemplateSnapshot TemplateSnapshot { get; private set; }

        //Track Progress
        private readonly List<int> _CompletedStepOrders = new();
        public IReadOnlyList<int> CompletedStepOrders => _CompletedStepOrders.AsReadOnly();
        // Navigation property to the Answer table
        private readonly List<ApplicationAnswer> _Answers = new List<ApplicationAnswer>();
        public IReadOnlyList<ApplicationAnswer> Answers => _Answers.AsReadOnly();

        public Guid UserId { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public Guid TemplateId { get; private set; }// frozen template
        public ApplicationStatus Status { get; private set; }// Draft, Submitted, Paid
        public PricingState PricingState { get; private set; }// Estimated / Locked
        public DateTime? SubmittedAt { get; private set; }
        public int CurrentStepOrder { get; private set; } = 1;

        public PricingResult? FinalPriceSummary { get; private set; }


        // =========================
        // Factory
        // =========================

        public static Result<MembershipApplication> Create(
            Guid id,
            string trackingNumber,
            ApplicationTemplateSnapshot templateSnapshot,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid templateId
            )
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return MembershipApplicationErrors.TrackingNumberRequired;

            if (userId == Guid.Empty)
                return MembershipApplicationErrors.UserIdRequired;

            if (clubId == Guid.Empty)
                return MembershipApplicationErrors.ClubIdRequired;


            return new MembershipApplication(
                id,
                trackingNumber,
                templateSnapshot,
                userId,
                clubId,
                membershipTypeId,
                membershipPlanId,
                templateId
                );
        }

        // =========================
        // Business Methods
        // =========================

        #region Answer Management Logic

        public Result<Success> CompleteStep(int stepOrder, List<ApplicationAnswer> newAnswers)
        {
            if (Status != ApplicationStatus.Draft)
                return MembershipApplicationErrors.InvalidStatusTransition;

            if (stepOrder > 1 && !_CompletedStepOrders.Contains(stepOrder - 1))
                return MembershipApplicationErrors.PreviousStepRequired;

            var step = TemplateSnapshot.Steps.FirstOrDefault(s => s.Order == stepOrder);
            if (step == null) return MembershipApplicationErrors.StepNotFound;

            var fields = step.Sections.SelectMany(s => s.Fields);
            // 1. Identify Fields in this step using the Snapshot to reset them
            var fieldIdsInStep = fields.Select(f => f.Id).ToList();
            _Answers.RemoveAll(a => fieldIdsInStep.Contains(a.FieldDefinitionId));

            // 2. Map and Validate
            foreach (var answer in newAnswers)
            {
                var fieldSnapshot = fields.FirstOrDefault(f => f.Id == answer.FieldDefinitionId);
                if (fieldSnapshot == null) continue;

                // Validate against frozen rules
                var validationErrors = fieldSnapshot.Validate(answer.Value);
                if (validationErrors.Any()) return validationErrors;

                // 3. Add the answer
                _Answers.Add(answer);
            }

            // 4. Update Progress
            if (!_CompletedStepOrders.Contains(stepOrder))
                _CompletedStepOrders.Add(stepOrder);

            MoveToStep(stepOrder+1);
            // 5. Update the live Price property
            RefreshPrice();

            return Result.Success;
        }



        private Result<Success> ValidateSectionCounts()
        {
            foreach (var step in TemplateSnapshot.Steps)
            {
                foreach (var section in step.Sections.Where(s => s.RepeatRule != null))
                {
                    // Find the value of the "Driver" field (e.g., guest_count)
                    var driverValue = _Answers.FirstOrDefault(a =>
                        a.FieldKey == section.RepeatRule!.DependsOnFieldKey &&
                        a.InstanceIndex == 0)?.Value;

                    // Use your Evaluate logic: ExactValue, AtLeastOne, etc.
                    int expectedCount = section.RepeatRule!.Evaluate(driverValue);

                    // Count unique indices for fields belonging to this section
                    var sectionFieldIds = section.Fields.Select(f => f.Id).ToList();
                    var actualCount = _Answers
                        .Where(a => sectionFieldIds.Contains(a.FieldDefinitionId))
                        .Select(a => a.InstanceIndex)
                        .Distinct()
                        .Count();

                    if (actualCount != expectedCount)
                        return MembershipApplicationErrors.SectionCountMismatch(section.Title,actualCount,expectedCount);
                }
            }
            return Result.Success;
        }


        #endregion


        // --- Business Logic: Pricing ---
        private PricingResult RefreshPrice()
        {
            // Get keys that the engine actually cares about
            var requiredKeys = PricingEngine.GetRequiredContextKeys(TemplateSnapshot.Policies);

            // Build context from drivers. 
            // Drivers (like guest_count) always live in InstanceIndex 0.
            var contextData = _Answers
                .Where(a => a.InstanceIndex == 0 && requiredKeys.Contains(a.FieldKey))
                .ToDictionary(a => a.FieldKey, a => a.Value);

            var result = PricingEngine.Calculate(TemplateSnapshot.BaseFee, TemplateSnapshot.Policies, new PricingContext(contextData));

            return result;
        }

        public Result<PricingResult> GetPricePreview()
        {

            // If locked, return the saved historical summary
            if (PricingState == PricingState.Locked)
            {
                return FinalPriceSummary != null ? FinalPriceSummary : Error.NotFound(description: "Locked price summary not found.");
            }
            return RefreshPrice();
        }

        // --- Business Logic: Submission ---
        public Result<Success> Submit()
        {
            // 1. Check Completeness
            var totalSteps = TemplateSnapshot.Steps.Count;
            if (_CompletedStepOrders.Count < totalSteps)
                return MembershipApplicationErrors.NotAllStepsCompleted;

            // 2. Check Repeat Integrity (The Driver Count vs Actual Detail Forms)
            var countValidation = ValidateSectionCounts();
            if (countValidation.IsError) return countValidation;

            // 3. Generate Immutable Receipt
            var finalResult = RefreshPrice();
            this.FinalPriceSummary = finalResult;

            // 4. Finalize State
            Status = ApplicationStatus.Submitted;
            PricingState = PricingState.Locked;

            return Result.Success;
        }

        //Navigate between steps
        public void MoveToStep(int stepOrder)
        {
            var totalSteps = TemplateSnapshot.Steps.Count;

            // 1. Calculate the "High Water Mark" (Furthest they can go)
            // They can go to any completed step, or one step past the highest completed step.
            var maxAllowed = _CompletedStepOrders.Any()
                ? Math.Min(_CompletedStepOrders.Max() + 1, totalSteps)
                : 1;

            // 2. Validate and Update
            if (stepOrder >= 1 && stepOrder <= maxAllowed)
            {
                CurrentStepOrder = stepOrder;
            }
        }
    }
}
