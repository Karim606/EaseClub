using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{

    public static class InstallmentEngine
    {
        public static Result<List<InstallmentBlueprint>> GenerateMembershipInstallments(
            IEnumerable<Installment> rules, // Changed to accept the list directly
            decimal totalPrice,
            DateTime? startDate = null)
        {
            // 1. Validation: Catch bad rules before starting math
            var validation = Validate(rules);
            if (validation.IsError) return validation.TopError;

            var sortedRules = rules.OrderBy(x => x.OrderIndex).ToList();
            var blueprints = new List<InstallmentBlueprint>();
            decimal runningTotal = 0;
            DateTime currentReferenceDate = startDate ?? DateTime.UtcNow;

            for (int i = 0; i < sortedRules.Count; i++)
            {
                var item = sortedRules[i];
                decimal installmentAmount;

                if (i == sortedRules.Count - 1)
                {
                    // The "Remainder Allocation" trick: Ensures total matches exactly
                    installmentAmount = totalPrice - runningTotal;
                }
                else
                {
                    installmentAmount = Math.Round((totalPrice * item.PercentageOfAmount) / 100, 2);
                    runningTotal += installmentAmount;
                }

                currentReferenceDate = currentReferenceDate.AddDays(item.DueAfterDays);

                blueprints.Add(new InstallmentBlueprint(
                    item.OrderIndex,
                    installmentAmount,
                    currentReferenceDate,
                    item.DueAfterDays,
                    item.PercentageOfAmount
                ));
            }

            return blueprints;
        }

        private static Result<Success> Validate(IEnumerable<Installment> installments)
        {
            // Check 1: Percentage Summation (allowing small float tolerance)
            if (Math.Abs(installments.Sum(x => x.PercentageOfAmount) - 100m) > 0.01m)
                return Error.Validation("Installments.InvalidPercentage", "Percentages must sum to 100%.");

            // Check 2: Sequential Ordering
            var sorted = installments.OrderBy(x => x.OrderIndex).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].OrderIndex != i )
                    return Error.Validation("Installments.InvalidOrder", "Order indexes must be sequential (1, 2, 3...).");
            }

            return Result.Success;
        }
    }
    // A simple DTO (Value Object) for the preview
    public record InstallmentBlueprint
    {
        public InstallmentBlueprint(
        int Order,
        decimal Amount,
        DateTime DueDate,
        int AfterDueInDays,
        decimal Percentage // Original % of the total price
        )
        {

            this.Order = Order;
            this.Amount = Amount;
            this.DueDate = DueDate;
            this.AfterDueInDays = AfterDueInDays;
            this.Percentage = Percentage;
        }
            public int Order { get; } 
            public decimal Amount { get; } 
            public DateTime DueDate { get; } 
            public int AfterDueInDays { get; } 
            public decimal Percentage { get; }

        public static Result<List<Installment>> ToInstallments(List<InstallmentBlueprint> list)
        {
            var installments = new List<Installment>();

            foreach (var item in list)
            {
                var result = Installment.Create(item.Percentage, item.AfterDueInDays, item.Order);

                if (result.IsError)
                    return result.TopError; // Return the specific domain error encountered

                installments.Add(result.Value);
            }

            return installments;
        }


    }
}
