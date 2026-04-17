using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.Memberships.ValueObjects;
using EaseClub.Domain.PricingPolices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Memberships
{
    public class MembershipCycle : AuditableEntity
    {
        public Guid MembershipId { get; private set; }
        public Membership Membership { get; private set; }
        public MembershipPeriod Period { get; private set; }

        private readonly List<MembershipInstallment> _Installments = new();
        public IReadOnlyList<MembershipInstallment> Installments => _Installments.AsReadOnly();
        private MembershipCycle() { }

        public MembershipCycle(Guid id, Guid membershipId, MembershipPeriod period):base(id)
        {
            MembershipId = membershipId;
            Period = period;
        }



        #region Factory

        public static Result<MembershipCycle> Create(
            Guid membershipId,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            DateTime startDate,
            DateTime endDate,
            decimal price,
            Guid? installmentTemplateId=null,
            List<Installment>? installments = null)
        {
            if (membershipId == Guid.Empty)
                return Error.Validation("MembershipCycle.MembershipId.Required", "Membership ID is required.");

            var period = MembershipPeriod.Create(startDate, endDate);
            if (period.IsError)
                return period.TopError;

            var cycle = new MembershipCycle(Guid.NewGuid(), membershipId,period.Value);

            // Optional: generate installments if passed
            if (installments != null && installments.Any())
            {
                var setupResult = cycle.SetupInstallments(
                    installments,
                    price,
                    clubId,
                    membershipTypeId,
                    membershipPlanId,
                    installmentTemplateId);
                if (setupResult.IsError)
                    return setupResult.TopError;
            }

            return cycle;
        }

        private Result<Success> SetupInstallments(
            List<Installment> installments,
            decimal price,
            Guid clubId,
            Guid membershipTypeId,
            Guid membershipPlanId,
            Guid? installmentTemplateId = null)
        {
            var instBluePrint = InstallmentEngine.GenerateMembershipInstallments(
                installments,
                price
                );

            if (instBluePrint.IsError) return instBluePrint.TopError;

            foreach (var inst in instBluePrint.Value)
            {
                var result = MembershipInstallment.Create(
                    Id,
                    clubId,
                    membershipTypeId,
                    membershipPlanId,
                    installmentTemplateId,
                    inst.Order,
                    inst.Amount,
                    inst.DueDate
                );

                if (result.IsError)
                    return result.TopError;

                _Installments.Add(result.Value);
            }

            return Result.Success;
        }

        #endregion
    }

}
