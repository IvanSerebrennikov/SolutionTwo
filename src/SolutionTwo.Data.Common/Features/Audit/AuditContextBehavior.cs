using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SolutionTwo.Common.LoggedInUserAccessor.Interfaces;
using SolutionTwo.Data.Common.Context;

namespace SolutionTwo.Data.Common.Features.Audit;

public class AuditContextBehavior : IAuditContextBehavior
{
    private readonly ILoggedInUserGetter _loggedInUserGetter;

    public AuditContextBehavior(ILoggedInUserGetter loggedInUserGetter)
    {
        _loggedInUserGetter = loggedInUserGetter;
    }

    public void AddGlobalQueryFilter(BaseDbContext context, ModelBuilder modelBuilder)
    {
        // do nothing here
    }

    public void BeforeSaveChanges(BaseDbContext context)
    {
        var dateTimeNow = DateTime.Now;
        var loggedInUserId = _loggedInUserGetter.UserId;
        
        foreach (var entry in context.ChangeTracker.Entries<IAuditableOnCreateEntity>()
                     .Where(entry => entry.State == EntityState.Added))
        {
            entry.Entity.CreatedDateTimeUtc = dateTimeNow;
            entry.Entity.CreatedBy = loggedInUserId ?? Guid.Empty;
        }

        foreach (var entry in context.ChangeTracker.Entries<IAuditableOnUpdateEntity>()
                     .Where(entry => entry.State == EntityState.Modified))
        {
            if (entry.Properties.Any(p =>
                    p.IsModified && p.Metadata.PropertyInfo != null &&
                    p.Metadata.PropertyInfo.GetCustomAttribute(typeof(IgnoreAuditAttribute)) == null))
            {
                entry.Entity.LastModifiedDateTimeUtc = dateTimeNow;
                entry.Entity.LastModifiedBy = loggedInUserId ?? Guid.Empty;
                entry.Property(nameof(entry.Entity.LastModifiedDateTimeUtc)).IsModified = true;
                entry.Property(nameof(entry.Entity.LastModifiedBy)).IsModified = true;
            }
        }
    }
}