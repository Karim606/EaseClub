using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EaseClub.Domain.PricingPolices
{
    public record PricingContext(Dictionary<string, string?> Data);

    public record PricingResult
    {
        private PricingResult() { }
        [JsonConstructor]
        public  PricingResult(
        decimal basePrice,
        decimal totalPrice,
        List<AppliedPolicyDetail> appliedPolicies)
        {
            BasePrice = basePrice;
            TotalPrice = totalPrice;
            AppliedPolicies = appliedPolicies;
        }

        public decimal BasePrice { get; private set; }

        public decimal TotalPrice { get; private set; }

        //[NotMapped]
        public List<AppliedPolicyDetail> AppliedPolicies { get; private set; } = new();

        //public string AppliedPoliciesJson
        //{
        //    get => JsonSerializer.Serialize(AppliedPolicies, new JsonSerializerOptions());
        //    set => AppliedPolicies = string.IsNullOrWhiteSpace(value)
        //        ? new List<AppliedPolicyDetail>()
        //        : JsonSerializer.Deserialize<List<AppliedPolicyDetail>>(value, new JsonSerializerOptions())!;
        //}

    }

    public record AppliedPolicyDetail {

        private AppliedPolicyDetail() { }
        [JsonConstructor]
        public AppliedPolicyDetail(string name, decimal adjustment)
        {
            Name = name;
            Adjustment = adjustment;
        }

        public string Name { get; private set; }

        public decimal Adjustment { get; private set; }
    };
}
