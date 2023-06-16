using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.ContextBehaviors.Interfaces;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Extensions;

namespace SolutionTwo.Data.Common.ContextBehaviors;

public class SoftDeletionContextBehavior : ISoftDeletionContextBehavior
{
    public void AddGlobalQueryFilter(BaseDbContext context, ModelBuilder modelBuilder)
    {
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletableEntity>(x => !x.IsDeleted);
    }

    public void BeforeSaveChanges(BaseDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletableEntity>()
                     .Where(entry => entry.State == EntityState.Deleted))
        {
            entry.State = EntityState.Unchanged;
            entry.Entity.IsDeleted = true;
            entry.Property(nameof(entry.Entity.IsDeleted)).IsModified = true;
        }
    }
}