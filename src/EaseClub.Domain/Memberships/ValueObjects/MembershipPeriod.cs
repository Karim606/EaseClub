using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Memberships.Errors;

namespace EaseClub.Domain.Memberships.ValueObjects
{
    public record MembershipPeriod
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

        private MembershipPeriod(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public static Result<MembershipPeriod> Create(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                return MembershipErrors.InvalidDateRange;

            return new MembershipPeriod(startDate, endDate);
        }

        public bool IsActive(DateTime now)
        {
            return now >= StartDate && now <= EndDate;
        }

        public bool IsExpired(DateTime now)
        {
            return now > EndDate;
        }

        public bool IsFuture(DateTime now)
        {
            return now < StartDate;
        }

        public int DurationInDays()
        {
            return (EndDate - StartDate).Days;
        }

        public int DurationInMonths()
        {
            return ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
        }

        public MembershipPeriod Extend(int days)
        {
            return new MembershipPeriod(StartDate, EndDate.AddDays(days));
        }

        public MembershipPeriod ExtendByMonths(int months)
        {
            return new MembershipPeriod(StartDate, EndDate.AddMonths(months));
        }

        public static MembershipPeriod FromDuration(DateTime startDate, int durationInMonths)
        {
            return new MembershipPeriod(startDate, startDate.AddMonths(durationInMonths));
        }
    }
}