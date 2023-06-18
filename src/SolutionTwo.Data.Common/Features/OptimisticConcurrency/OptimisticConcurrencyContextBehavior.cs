using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Context;

namespace SolutionTwo.Data.Common.Features.OptimisticConcurrency;

public class OptimisticConcurrencyContextBehavior : IOptimisticConcurrencyContextBehavior
{
    public void AddGlobalQueryFilter(BaseDbContext context, ModelBuilder modelBuilder)
    {
        // do nothing here
    }

    public void BeforeSaveChanges(BaseDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<IVersionedEntity>())
        {
            if (entry.State == EntityState.Deleted ||
                (entry.State == EntityState.Modified && entry.Properties.Any(p =>
                    p.IsModified && 
                    p.Metadata.PropertyInfo != null &&
                    p.Metadata.PropertyInfo
                        .GetCustomAttribute(typeof(ConcurrencyVersionChangedOnUpdateAttribute)) != null)))
            {
                entry.Entity.ConcurrencyVersion = Guid.NewGuid();
                entry.Property(nameof(entry.Entity.ConcurrencyVersion)).IsModified = true;
            }
        }
    }
}