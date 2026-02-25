using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipApplications
{
    public class MembershipApplication:AuditableEntity
    {
        private MembershipApplication() { }

        private MembershipApplication(
            Guid id,
            string trackingNumber,
            string templateSnapshot,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid templateId,
            decimal basePrice)
            : base(id)
        {
            TrackingNumber = trackingNumber;
            TemplateSnapshot = templateSnapshot;
            UserId = userId;
            ClubId = clubId;
            MembershipTypeId = membershipTypeId;
            MembershipPlanId = membershipPlanId;
            TemplateId = templateId;
            BasePrice = basePrice;
            FinalPrice = basePrice;
            Status = ApplicationStatus.Draft;
            PricingState = PricingState.Estimated;
            CreatedAt = DateTime.UtcNow;
        }
        public string TrackingNumber { get; private set; }
        public string TemplateSnapshot { get; private set; }

        // Navigation property to the Answer table
        private readonly List<ApplicationAnswer> _Answers = new List<ApplicationAnswer>();
        public IReadOnlyList<ApplicationAnswer> Answers => _Answers.AsReadOnly();

        public Guid UserId { get; private set; }
        public Guid ClubId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public Guid MembershipPlanId { get; private set; }
        public Guid TemplateId { get; private set; }// frozen template
        public ApplicationStatus Status { get; private set; }// Draft, Submitted, Paid
        public decimal BasePrice { get; private set; }
        public decimal FinalPrice { get; private set; }
        public PricingState PricingState { get; private set; }// Estimated / Locked
        public DateTime CreatedAt { get; private set; }
        public DateTime? SubmittedAt { get; private set; }


        // =========================
        // Factory
        // =========================

        public static Result<MembershipApplication> Create(
            Guid id,
            string trackingNumber,
            string templateSnapshot,
            Guid userId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid templateId,
            decimal basePrice)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return MembershipApplicationErrors.TrackingNumberRequired;

            if (userId == Guid.Empty)
                return MembershipApplicationErrors.UserIdRequired;

            if (clubId == Guid.Empty)
                return MembershipApplicationErrors.ClubIdRequired;

            if (basePrice < 0)
                return MembershipApplicationErrors.InvalidBasePrice;

            return new MembershipApplication(
                id,
                trackingNumber,
                templateSnapshot,
                userId,
                clubId,
                membershipTypeId,
                membershipPlanId,
                templateId,
                basePrice);
        }

        // =========================
        // Business Methods
        // =========================

        #region Answer Management Logic

        /// <summary>
        /// Adds or Updates an answer for a specific field.
        /// In a snapshot/key-value model, "Add" and "Update" are often the same operation.
        /// </summary>
        public Result<Success> SetAnswer(string fieldKey,Guid fieldDefintionId, string value, int instanceIndex = 0)
        {
            if (Status != ApplicationStatus.Draft)
                return Error.Validation("Application.NotEditable", "Cannot modify answers after submission.");

            var existing = _Answers.FirstOrDefault(a =>
                a.FieldDefinitionId == fieldDefintionId &&
                a.InstanceIndex == instanceIndex);

            if (existing != null)
            {
                existing.UpdateValue(value);
            }
            else
            {
                var answerResult = ApplicationAnswer.Create(Id,fieldDefintionId, fieldKey, value, instanceIndex);
                if (answerResult.IsError) return answerResult.TopError;

                _Answers.Add(answerResult.Value);
            }

            return Result.Success;
        }

        /// <summary>
        /// Removes a specific answer. 
        /// Useful if a user clears a field or deletes a repeatable section instance.
        /// </summary>
        public Result<Success> RemoveAnswer(Guid fieldDefintionId, int instanceIndex = 0)
        {
            if (Status != ApplicationStatus.Draft)
                return Error.Validation("Application.NotEditable", "Cannot modify answers after submission.");

            var answer = _Answers.FirstOrDefault(a =>
                a.FieldDefinitionId == fieldDefintionId &&
                a.InstanceIndex == instanceIndex);

            if (answer != null)
            {
                _Answers.Remove(answer);
            }

            return Result.Success;
        }

        /// <summary>
        /// Removes all answers associated with a specific index.
        /// Used when a user deletes an entire "Repeatable Section" instance (e.g., Delete Child #2).
        /// </summary>
        public void RemoveAllAnswersForIndex(int instanceIndex)
        {
            _Answers.RemoveAll(a => a.InstanceIndex == instanceIndex);
        }
        public void RemoveSectionInstance(Guid sectionId, int instanceIndex)
        {
            if (Status != ApplicationStatus.Draft) return;

            // 1. Identify which fields belong to this section from the Snapshot
            var fieldsInSection = GetFieldsForSectionFromSnapshot(sectionId);

            // 2. Remove all answers for the specific instance index
            _Answers.RemoveAll(a =>
                fieldsInSection.Contains(a.FieldDefinitionId) &&
                a.InstanceIndex == instanceIndex);

            // 3. RE-INDEXING LOGIC: Close the gap
            // If we deleted index 1, shift index 2 -> 1, 3 -> 2, etc.
            var answersToShift = _Answers
                .Where(a => fieldsInSection.Contains(a.FieldDefinitionId) && a.InstanceIndex > instanceIndex)
                .ToList();

            foreach (var answer in answersToShift)
            {
                // We need an internal method in ApplicationAnswer to update the index
                answer.UpdateInstanceIndex(answer.InstanceIndex - 1);
            }
        }

        private List<Guid> GetFieldsForSectionFromSnapshot(Guid sectionId)
        {
            var fieldIds = new List<Guid>();

            // Performance optimization: Using 'using' locally is fine, 
            // but if this is called in a loop, consider passing the parsed JsonElement in.
            using var doc = JsonDocument.Parse(TemplateSnapshot);
            var steps = doc.RootElement.GetProperty("Steps");

            foreach (var step in steps.EnumerateArray())
            {
                foreach (var section in step.GetProperty("Sections").EnumerateArray())
                {
                    if (section.GetProperty("Id").GetGuid() == sectionId)
                    {
                        foreach (var field in section.GetProperty("Fields").EnumerateArray())
                        {
                            fieldIds.Add(field.GetProperty("Id").GetGuid());
                        }
                        return fieldIds;
                    }
                }
            }
            return fieldIds;
        }
        #endregion

        public Result<Success> Submit()
        {
            if (Status != ApplicationStatus.Draft)
                return MembershipApplicationErrors.InvalidStatusTransition;

            Status = ApplicationStatus.Submitted;
            SubmittedAt = DateTime.UtcNow;
            PricingState = PricingState.Locked;

            return Result.Success;
        }

        public Result<Success> ApplyPricing(decimal newPrice)
        {
            if (PricingState == PricingState.Locked)
                return MembershipApplicationErrors.PricingLocked; 

            FinalPrice = newPrice;
            return Result.Success;
        }

    }
}
