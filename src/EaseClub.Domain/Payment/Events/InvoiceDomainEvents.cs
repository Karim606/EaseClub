using EaseClub.Domain.Clubs;
using EaseClub.Domain.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Payment.Events
{
    public record InvoicePaidEvent(Guid Id, Guid PayableId, PayableType type, decimal Amount,Guid  UserId, Guid ClubId):DomainEvent;
}
