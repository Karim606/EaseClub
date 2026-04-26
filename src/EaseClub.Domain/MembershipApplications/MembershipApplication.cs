using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.PricingPolices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications
{
    public class MembershipApplication : AuditableEntity,IHaveClub,IBelongToMember
    {
        private MembershipApplication() { }

        private MembershipApplication(
            Guid id,
            string trackingNumber,
            ApplicationTemplateSnapshot templateSnapshot,
            Guid memberId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid? installmentTemplateId,
            Guid templateId
           )
            : base(id)
        {
            TrackingNumber = trackingNumber;
            TemplateSnapshot = templateSnapshot;
            MemberId = memberId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            InstallmentTemplateId = installmentTemplateId;
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

        private readonly List<ApplicationReview> _Reviews = new();
        public IReadOnlyList<ApplicationReview> Reviews => _Reviews.AsReadOnly();


        public Guid MemberId { get; private set; }
        public MemberUser Member { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid? InstallmentTemplateId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public MembershipType MembershipType { get; private set; }
        public Club  Club { get; private set; }
        public MembershipPlan MembershipPlan { get; private set; }
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
            MembershipPlan plan,
            InstallmentTemplate? installmentTemplate,
            MembershipType membershipType,
            Guid templateId
            )
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return MembershipApplicationErrors.TrackingNumberRequired;

            if(plan.MembershipTypeId != membershipType.Id) return Error.Conflict(description: "MembershipPlan isnt associated with this MembershipType");

            if(installmentTemplate != null && !plan.InstallmentTemplates.Any(i => i.InstallmentTemplateId == installmentTemplate.Id)) 
                return Error.Conflict(description:"InstallmentTemplate isnt associated with this MembershipPlan");
            
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
                membershipType.Id,
                plan.Id,
                installmentTemplate?.Id,
                templateId
                );
        }

        // =========================
        // Business Methods
        // =========================

        #region Answer Management Logic

        public Result<Success> CompleteStep(int stepOrder, List<ApplicationAnswer> newAnswers)
        {
            // 1. Pre-validation: Status & Template existence
            if (Status != ApplicationStatus.Draft)
                return MembershipApplicationErrors.InvalidStatusTransition;

            var step = TemplateSnapshot.Steps.FirstOrDefault(s => s.Order == stepOrder);
            if (step == null) return MembershipApplicationErrors.StepNotFound;

            // 2. Identify permitted fields (Zero Trust)
            var fieldIdsInStep = step.Sections.SelectMany(s => s.Fields).Select(f => f.Id).ToHashSet();
            var filteredAnswers = newAnswers.Where(a => fieldIdsInStep.Contains(a.FieldDefinitionId)).ToList();

            var errors = new List<Error>();

            // 3. Validation Pipeline
            ValidateRepeatRules(step, filteredAnswers, errors);
            ValidateStructuralIntegrity(step, filteredAnswers, errors);

            if (errors.Any()) return errors;

            // 4. Persistence & State Progression
            ApplyAnswersToStep(fieldIdsInStep, filteredAnswers);
            UpdateProgress(stepOrder);
            RefreshPrice();

            return Result.Success;
        }

        #region Private Helpers

        private void ValidateRepeatRules(StepSnapshot step, List<ApplicationAnswer> answers, List<Error> errors)
        {
            foreach (var section in step.Sections)
            {
                if (section.RepeatRule == null) continue;

                var sectionFieldIds = section.Fields.Select(f => f.Id).ToHashSet();
                var uniqueInstancesCount = answers
                    .Where(a => sectionFieldIds.Contains(a.FieldDefinitionId))
                    .Select(a => a.InstanceIndex)
                    .Distinct()
                    .Count();

                if (!section.RepeatRule.Evaluate(uniqueInstancesCount))
                {
                    errors.Add(Error.Validation(
                        "Application.InvalidRepeatCount",
                        $"Section '{section.Title}' requires {section.RepeatRule.Mode} ({section.RepeatRule.NumberOfRepeats}), but received {uniqueInstancesCount}."));
                }
            }
        }

        private void ValidateStructuralIntegrity(StepSnapshot step, List<ApplicationAnswer> answers, List<Error> errors)
        {
            foreach (var section in step.Sections)
            {
                var sectionFields = section.Fields;
                var sectionFieldIds = sectionFields.Select(f => f.Id).ToHashSet();

                var instances = answers
                    .Where(a => sectionFieldIds.Contains(a.FieldDefinitionId))
                    .GroupBy(a => a.InstanceIndex)
                    .OrderBy(g => g.Key)
                    .ToList();

                // Start at 1 to match the 1-based InstanceIndex
                for (int i = 1; i <= instances.Count; i++)
                {
                    // Now 'i' is exactly the index we expect (1, 2, 3...)
                    var currentInstance = instances[i - 1]; // Access the list via 0-based offset

                    if (currentInstance.Key != i)
                    {
                        errors.Add(Error.Validation("Application.StructuralGap",
                            $"Sequence gap in '{section.Title}': expected instance {i} but found {currentInstance.Key}."));
                        return;
                    }

                    var instanceAnswers = currentInstance.ToDictionary(a => a.FieldDefinitionId);

                    foreach (var fieldDef in sectionFields)
                    {
                        bool provided = instanceAnswers.TryGetValue(fieldDef.Id, out var answer);
                        bool hasValue = provided && !string.IsNullOrWhiteSpace(answer?.Value);

                        if (fieldDef.ValidationRules.IsRequired && !hasValue)
                        {
                            errors.Add(Error.Validation("Application.MissingRequiredField",
                                $"'{fieldDef.Label}' is required for {section.Title} item #{i}."));
                        }

                        if (provided)
                        {
                            var fieldErrors = fieldDef.Validate(answer!.Value);
                            errors.AddRange(fieldErrors);
                        }
                    }
                }
            }
        }

        private void ApplyAnswersToStep(HashSet<Guid> fieldIdsInStep, List<ApplicationAnswer> filteredAnswers)
        {
            _Answers.RemoveAll(a => fieldIdsInStep.Contains(a.FieldDefinitionId));
            _Answers.AddRange(filteredAnswers);
        }

        private void UpdateProgress(int stepOrder)
        {
            if (!_CompletedStepOrders.Contains(stepOrder))
                _CompletedStepOrders.Add(stepOrder);

            _CompletedStepOrders.RemoveAll(order => order > stepOrder);
            CurrentStepOrder = stepOrder + 1;
        }

        #endregion

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
            FinalPriceSummary = RefreshPrice();
            return FinalPriceSummary;
        }

        public Result<List<InstallmentBlueprint>>GetPaymentSchedule(){
            
            var res = GetPricePreview();

            if (res.IsError) return res.TopError;

            var finalPrice= FinalPriceSummary!.TotalPrice;

            List<Installment> installments = new();
            foreach(var inst in TemplateSnapshot.InstallmentRules)
            {
                installments.Add(inst.ToDomain());
            }
           var resultedInstallments = InstallmentEngine.GenerateMembershipInstallments(installments, finalPrice, DateTime.Now);

            if(resultedInstallments.IsError) return resultedInstallments.TopError;

            return resultedInstallments.Value;

        }

        // --- Business Logic: Submission ---
        public Result<Success> Submit()
        {
            // 1. Check Completeness
            var totalSteps = TemplateSnapshot.Steps.Count;
            if (_CompletedStepOrders.Count < totalSteps)
                return MembershipApplicationErrors.NotAllStepsCompleted;

            // 3. Generate Immutable Receipt
            var finalResult = RefreshPrice();
            this.FinalPriceSummary = finalResult;

            // 4. Finalize State
            Status = ApplicationStatus.Submitted;
            PricingState = PricingState.Locked;

            return Result.Success;
        }


        // Add a new review (approval or rejection)
        public Result<Success> AddReview(ApplicationReview review)
        {
            // Business rules
            if (Status != ApplicationStatus.Submitted)
                return Error.Conflict(description:"Only submitted applications can be reviewed");

            

            // Add review to collection
            _Reviews.Add(review);

            // Update status based on decision
            Status = review.Decision switch
            {
                DecisionsAboutApplication.Approved => ApplicationStatus.Approved,
                DecisionsAboutApplication.Rejected => ApplicationStatus.Rejected,
                _ => Status
            };

            // Raise domain events

            if (review.Decision == DecisionsAboutApplication.Approved)
                RaiseDomainEvent(new ApplicationApprovedEvent(MemberId,Id, review.Id,review.Note));

            if (review.Decision == DecisionsAboutApplication.Rejected)
                RaiseDomainEvent(new ApplicationRejectedEvent(MemberId,Id, review.Id, review.Reason!));

            return Result.Success;
        }

    }
}
