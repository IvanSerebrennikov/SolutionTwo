using Microsoft.EntityFrameworkCore;
using SolutionTwo.Common.LoggedInUserAccessor.Interfaces;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.ContextBehaviors.Interfaces;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Extensions;

namespace SolutionTwo.Data.Common.ContextBehaviors;

public class SoftDeletionContextBehavior : ISoftDeletionContextBehavior
{
    private readonly ILoggedInUserGetter _loggedInUserGetter;

    public SoftDeletionContextBehavior(ILoggedInUserGetter loggedInUserGetter)
    {
        _loggedInUserGetter = loggedInUserGetter;
    }

    public void AddGlobalQueryFilter(BaseDbContext context, ModelBuilder modelBuilder)
    {
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletableEntity>(x => x.DeletedDateTimeUtc == null);
    }

    public void BeforeSaveChanges(BaseDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletableEntity>()
                     .Where(entry => entry.State == EntityState.Deleted))
        {
            entry.State = EntityState.Unchanged;
            
            entry.Entity.DeletedDateTimeUtc = DateTime.UtcNow;
            entry.Property(nameof(entry.Entity.DeletedDateTimeUtc)).IsModified = true;

            var deletedByUserId = _loggedInUserGetter.UserId;
            if (deletedByUserId != null)
            {
                entry.Entity.DeletedBy = deletedByUserId;
                entry.Property(nameof(entry.Entity.DeletedBy)).IsModified = true;
            }
        }
    }
}