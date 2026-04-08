using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment
{
    public static class BillingITemIdGenerator
    {
        public static string Generate(
            BillingItemType type,
            Guid userId,
            Guid clubId,
            Guid payableId,
            DateTime dueDate)
        {
            // type abbreviation
            var typeAbbr = type.ToString().Substring(0, 3).ToUpper();

            // year
            var year = dueDate.Year;

            // deterministic hash (short, human-readable)
            var input = $"{userId}-{clubId}-{payableId}-{dueDate:yyyyMMddHHmmss}";
            using var sha = System.Security.Cryptography.SHA1.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Take first 3 bytes as hex
            var hashPart = BitConverter.ToUInt32(hashBytes, 0) & 0xFFFFFF; // 3 bytes
            var hashStr = hashPart.ToString("X6"); // 6-digit hex

            return $"{typeAbbr}-{year}-{hashStr}";
        }
    }
}
