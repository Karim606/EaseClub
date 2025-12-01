using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common
{
    public abstract class AuditableEntity:Entity
    {
        protected AuditableEntity() : base() { }
        protected AuditableEntity(Guid id) : base(id) { }
        public DateTime CreatedAt { get; private set; }
        public Guid CreatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        public void SetCreated(Guid userId)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
        }

        public void SetUpdated(Guid userId)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userId;
        }
    }
}
