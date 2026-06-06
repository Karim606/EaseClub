using EaseClub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Notifications
{
    public class Notification : Entity
    {
        public string Title { get; private set; }
        public string Message { get; private set; }

        public NotificationType Type { get; private set; }

        public Guid? UserId { get; private set; }     // specific user
        public Guid? ClubId { get; private set; }     // club-wide notification

        public DateTime CreatedAt { get; private set; }

        public bool IsRead { get; private set; }  

        public string? MetadataJson { get; private set; }

        private Notification() { }
        private Notification(Guid id,Guid? userId, Guid? clubId, string title, string message, NotificationType type, string? metadataJson): base(id)
        {
            UserId = userId;
            ClubId = clubId;
            Title = title;
            Message = message;
            Type = type;
            MetadataJson = metadataJson;
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
        public static Notification ForUser(Guid userId, string title, string message, NotificationType type, string? metadataJson = null)
        {
          return new  Notification(Guid.NewGuid(),userId, null, title, message, type, metadataJson);

        }

        public static Notification ForClub(Guid clubId, string title, string message, NotificationType type)
        {
            return new  Notification(Guid.NewGuid(),null, clubId, title, message, type, null);
        }
        

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
