using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment
{
    public static class InvoiceIdGenerator
    {
        public static string Generate()
        {
            var year = DateTime.UtcNow.Year;
            var random = Guid.NewGuid().ToString("N")[..6].ToUpper();

            return $"INV-{year}-{random}";
        }
    }
}
