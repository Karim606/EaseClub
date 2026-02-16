using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common
{
    public class Entity:IEntity
    {
        public Guid Id { get; }

        protected Entity() { }

        protected Entity(Guid id) { Id = id == Guid.Empty ? Guid.NewGuid() : id; }

        private readonly List<DomainEvent> _DomainEvents = new();

        [NotMapped]
        public IReadOnlyCollection<DomainEvent> DomainEvents => _DomainEvents.AsReadOnly();

        protected void RaiseDomainEvent(DomainEvent domainEvent) => _DomainEvents.Add(domainEvent);


        public void ClearDomainEvents() => _DomainEvents.Clear();
    }
}
