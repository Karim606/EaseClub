using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.MembershipPlans
{
    public class MembershipInstallment : AuditableEntity,IHaveClub
    {
        public Guid MembershipId { get; private set; }
        public Guid ClubId { get; private set; }
        public Club Club { get; private set; }
        public int Order { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime DueDate { get; private set; }
        public InstallmentStatus Status { get; private set; }
        public Guid? InvoiceId { get; private set; }

        private MembershipInstallment() { }

        public MembershipInstallment(Guid membershipId,Guid clubId, int order, decimal amount, DateTime dueDate)
        {
            MembershipId = membershipId;
            ClubId = clubId;
            Order = order;
            Amount = amount;
            DueDate = dueDate;
            Status = InstallmentStatus.Pending;
        }

        public Result<Success> MarkPaid(Guid invoiceId)
        {
            if (Status == InstallmentStatus.Pending || Status == InstallmentStatus.Overdue) { 
                Status = InstallmentStatus.Paid;
                InvoiceId = invoiceId;

                return Result.Success;
            }
            return MembershipInstallmentErrors.OnlyPendingInstallmentOrOverDueCanBePaid;
        }

        public Result<Success> MarkOverdue()
        {
            if(Status != InstallmentStatus.Pending)
                return MembershipInstallmentErrors.CantMarkUnPendingInstallmentAsOverDue;

            if (DueDate > DateTime.UtcNow)
                return MembershipInstallmentErrors.NotOverDuedYet;
            
            Status = InstallmentStatus.Overdue;
            return Result.Success;

        }

        public static Result<MembershipInstallment> Create(Guid membershipId,Guid clubId, int order, decimal amount, DateTime dueDate)
        {
            if (membershipId == Guid.Empty)
               return MembershipInstallmentErrors.MembershipIdMustBeProvided;

            if (clubId == Guid.Empty)
                return OwnedByClubErrors.ClubIdIsRequired;

            if(order < 0)
                return MembershipInstallmentErrors.InstallmentOrderMustBeNonNegative;
            
            if (amount <= 0)
                return  MembershipInstallmentErrors.InstallmentAmountMustBeGreaterThanZero;
           
            if (dueDate <= DateTime.UtcNow)
                return MembershipInstallmentErrors.InstallmentDueDateMustBeInTheFuture;

            return new MembershipInstallment(membershipId,clubId, order, amount, dueDate);
        }
    }
}
