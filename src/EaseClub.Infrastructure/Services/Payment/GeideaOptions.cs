using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services.Payment
{
    public class GeideaOptions
    {
        public const string SectionName = "Geidea";
        public string BaseUrl { get; set; } 
        public string MerchantId { get; set; } 
        public string ApiPassword { get; set; }  // Your Secret Key
        public string PublicKey { get; set; }    // merchantPublicKey
        public string SuccessUrl { get; set; }
        public string CallbackUrl { get; set; }
        public string Currency { get; set; } = "EGP";
    }
}
