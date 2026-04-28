using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.MembershipApplications.Enums;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications
{

    public record AnswerDto(Guid FieldId, string Key, FieldType FieldType, string Value, string? InstanceId = null);

    public record AnswerRequestDto(Guid FieldId, string Value, string? InstanceId = null);
    public class MembershipAppDto
    {
        public Guid ClubId { get; set; }
        public Guid UserId { get; set; }
        public string TrackingNumber { get; set; }

        public string MembershipType { get; set; }
        public string MembershipPlanName { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public ApplicationStatus Status { get; set; } //Status
        public Guid? EnrollmentInvoiceId { get; set; }

    }

    public class ReviewApplicationDto
    {
        public DecisionsAboutApplication Decision { get; set; }
        public string? Reason { get; set; }
    }
}
