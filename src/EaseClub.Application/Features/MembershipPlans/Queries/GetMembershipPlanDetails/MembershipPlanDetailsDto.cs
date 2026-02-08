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
    decimal Price,
    bool IsActive,
    List<InstallmentsTemplateDto> Templates);
}
