using EaseClub.Domain.MembershipPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Entities.MembershipPlan
{
    public static class InstallmentTemplateBuilder
    {
        public static InstallmentTemplate CreateValidTemplate()
        {
            var installments = new List<Installment>
        {
            Installment.Create(50m, 0, 0).Value,
            Installment.Create(50m, 30, 1).Value
        };

            return InstallmentTemplate.Create(
                Guid.NewGuid(),
                null,
                null,
                installments
            ).Value;
        }
    }
}
