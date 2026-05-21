using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common;

namespace EaseClub.Infrastructure.Data.Interceptors
{
    public class AuditableEntityInterceptor() : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
           UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void UpdateEntities(DbContext context)
        {
            if (context == null) return;
            var entries = context.ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in entries)
            {
                if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                

                    if(entry.State == EntityState.Added)
                    {
                        entry.Entity.SetCreated(Guid.Empty);
                    }
                    entry.Entity.SetUpdated(Guid.Empty);

                    foreach(var ownedEntry in entry.References)
                    {
                        var target = ownedEntry.TargetEntry;
                        if ( target is not {Entity: AuditableEntity ownedEntity} || target.State is not (EntityState.Added or EntityState.Modified))
                        {
                            continue;
                        }

                        if (target.State == EntityState.Added)
                        {
                            ownedEntity.SetCreated(Guid.Empty);
                        }

                        ownedEntity.SetUpdated(Guid.Empty);
                    }
                }
            }
        }
    }


    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry)
        {
            return entry.References.Any(r => r.TargetEntry != null && r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified || r.TargetEntry.HasChangedOwnedEntities()));
        }
    }
}
