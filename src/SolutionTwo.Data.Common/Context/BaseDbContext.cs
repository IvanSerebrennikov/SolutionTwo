using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Extensions;

namespace SolutionTwo.Data.Common.Context;

public class BaseDbContext : DbContext
{
    public BaseDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSoftDeletionBeforeSaveChanges();

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        HandleSoftDeletionBeforeSaveChanges();

        return base.SaveChanges();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletableEntity>(x => !x.IsDeleted);
    }

    private void HandleSoftDeletionBeforeSaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDeletableEntity>()
                     .Where(entry => entry.State == EntityState.Deleted))
        {
            entry.State = EntityState.Unchanged;
            entry.Entity.IsDeleted = true;
            entry.Property(nameof(entry.Entity.IsDeleted)).IsModified = true;
        }
    }
}