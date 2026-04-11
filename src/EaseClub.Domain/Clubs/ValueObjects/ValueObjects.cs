using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EaseClub.Domain.Clubs.ValueObjects
{
    public record Amenity {
    
        private Amenity() { }

        public Amenity(string name)
        {
            Name = name;
        }
        public string Name { get; init; }

    };

    // 2. Contact Information
    public record ContactInfo
    {
        private ContactInfo() { }
        public ContactInfo(PhoneNumber phone,Email email)
        {
            Email = email;
            Phone = phone;
        }
       public Email Email { get; init; }
       public PhoneNumber Phone { get; init; }
    }

    // 3. Work Hours (Using a dedicated object for day ranges)
    public record WorkSchedule
    {
        public string Label { get; init; }      // e.g., "Monday - Friday" or "Holidays"
        public string TimeRange { get; init; }  // e.g., "6:00 AM - 10:00 PM"

        private WorkSchedule(string label, string timeRange)
        {
            Label = label;
            TimeRange = timeRange;
        }

        public static Result<WorkSchedule> Create(string label, string timeRange)
        {
            if (string.IsNullOrWhiteSpace(label))
                return Error.Validation("Schedule label is required.");

            if (string.IsNullOrWhiteSpace(timeRange))
                return Error.Validation("Time range is required.");

            // Simple Regex check for: "6:00 AM - 10:00 PM"
            var timeRegex = @"^\d{1,2}:\d{2}\s?(AM|PM)\s?-\s?\d{1,2}:\d{2}\s?(AM|PM)$";
            if (!Regex.IsMatch(timeRange, timeRegex, RegexOptions.IgnoreCase))
            {
                return Error.Validation("Time range must be in format 'HH:mm AM/PM - HH:mm AM/PM'.");
            }

            return new WorkSchedule(label.Trim(), timeRange.Trim());
        }
    }
}
