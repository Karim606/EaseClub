using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Interfaces
{
    public interface IRequireMembership:IAuthorizeRequest
    {
        public Guid ClubId { get; set; }
    }
}
