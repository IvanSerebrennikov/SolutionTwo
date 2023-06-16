using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.ContextBehaviors.Base.Interfaces;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Extensions;

namespace SolutionTwo.Data.Common.ContextBehaviors;

public class SoftDeletionContextBehavior : IContextBehavior
{
    public void AddGlobalQueryFilter(ModelBuilder modelBuilder, BaseDbContext context, int behaviorIndex)
    {
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletableEntity>(x => !x.IsDeleted);
    }

    public void BeforeSaveChanges(ChangeTracker changeTracker)
    {
        foreach (var entry in changeTracker.Entries<ISoftDeletableEntity>()
                     .Where(entry => entry.State == EntityState.Deleted))
        {
            entry.State = EntityState.Unchanged;
            entry.Entity.IsDeleted = true;
            entry.Property(nameof(entry.Entity.IsDeleted)).IsModified = true;
        }
    }
}