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
                    p.IsModified && p.Metadata.PropertyInfo != null &&
                    p.Metadata.PropertyInfo.GetCustomAttribute(typeof(VersionChangedOnUpdateAttribute)) != null)))
            {
                entry.Entity.Version = Guid.NewGuid();
                entry.Property(nameof(entry.Entity.Version)).IsModified = true;
            }
        }
    }
}