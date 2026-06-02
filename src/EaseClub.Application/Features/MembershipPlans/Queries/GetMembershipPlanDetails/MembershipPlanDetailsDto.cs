using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipPlans.Queries.GetMembershipPlanDetails
{
    public record MembershipPlanDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    int MaxPaymentPeriodInDays,
    decimal Price,
    bool IsActive,
    List<InstallmentsTemplateDto> Templates,
    decimal RenewPrice,
    Guid MembershipTypeId,
    string PaymentMode,
    string EnrollmentMode,
    int SubscriptionValidityInYears,
    int MaxFamilyMembers);
}
