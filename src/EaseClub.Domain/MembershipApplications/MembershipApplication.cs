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
    public class MembershipApplication : AuditableEntity,IHaveClub,IBelongToUser
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
            Guid installmentTemplateId,
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


        public Guid UserId { get; private set; }
        public MemberUser User { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid InstallmentTemplateId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public MembershipType MembershipType { get; private set; }
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
            InstallmentTemplate installmentTemplate,
            MembershipType membershipType,
            Guid templateId
            )
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return MembershipApplicationErrors.TrackingNumberRequired;

            if(plan.MembershipTypeId != membershipType.Id) return Error.Conflict(description: "MembershipPlan isnt associated with this MembershipType");

            if(plan.InstallmentTemplates.All(i => i.InstallmentTemplateId != installmentTemplate.Id)) return Error.Conflict(description:"InstallmentTemplate isnt associated with this MembershipPlan");
            
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
                installmentTemplate.Id,
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
                // Only validate sections that have a RepeatRule
                foreach (var section in step.Sections.Where(s => s.RepeatRule != null))
                {
                    // 1. Get the actual number of instances the user submitted
                    var sectionFieldIds = section.Fields.Select(f => f.Id).ToList();
                    var actualCount = _Answers
                        .Where(a => sectionFieldIds.Contains(a.FieldDefinitionId))
                        .Select(a => a.InstanceIndex)
                        .Distinct()
                        .Count();

                    // 2. Evaluate the rule directly against actualCount
                    bool isValid = section.RepeatRule!.Evaluate(actualCount);

                    if (!isValid)
                    {
                        return MembershipApplicationErrors.SectionCountMismatch(
                            section.Title,
                            actualCount,
                            section.RepeatRule.NumberOfRepeats);
                    }
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
            FinalPriceSummary = RefreshPrice();
            return FinalPriceSummary;
        }

        public Result<List<Installment>>GetPaymentSchedule(){
            
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

            return InstallmentBlueprint.ToInstallments(resultedInstallments.Value);

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
                RaiseDomainEvent(new ApplicationApprovedEvent(UserId,Id, review.Id,review.Note));

            if (review.Decision == DecisionsAboutApplication.Rejected)
                RaiseDomainEvent(new ApplicationRejectedEvent(UserId,Id, review.Id, review.Reason!));

            return Result.Success;
        }

    }
}
