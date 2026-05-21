using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public record Installment
    {
        public decimal PercentageOfAmount { get; init; }
        public int DueAfterDays { get; init; }
        public int OrderIndex { get; init; }

        private Installment() {}
        private Installment(decimal percentageOfAmount, int dueAfterDays,int orderIndex)
        {

            PercentageOfAmount = percentageOfAmount;
            DueAfterDays = dueAfterDays;
            OrderIndex = orderIndex;
        }

        public static Result<Installment>Create(decimal percentageOfAmount, int dueAfterDays,int orderIndex)
        {
            if (percentageOfAmount <= 0 || percentageOfAmount > 100)
                return InstallmentErrors.InstallmentPercentageMustBeGreaterThanZeroAndLessThanOrEqual100;
            if (dueAfterDays < 0)
                return InstallmentErrors.InstallmentDueAfterDaysMustBeNonNegative;
            if(orderIndex <= 0)
                return InstallmentErrors.InstallmentOrderIndexMustBeGreaterThanZero;

            return new Installment(percentageOfAmount, dueAfterDays,orderIndex);
        }

        public InstallmentRuleSnapshot ToSnapshot() => InstallmentRuleSnapshot.FromDomain(this);

        public static List<InstallmentRuleSnapshot> ListToSnapshot(IEnumerable<Installment> list) {
            List<InstallmentRuleSnapshot> listOfSnapshots = new();
            foreach (var item in list)
            {
                listOfSnapshots.Add(item.ToSnapshot());
            }
            return listOfSnapshots;
            }

    }
}
