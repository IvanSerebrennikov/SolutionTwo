using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.ContextBehaviors.Base.Interfaces;

namespace SolutionTwo.Data.Common.Context;

public class BaseDbContext : DbContext
{
    public IReadOnlyList<IContextBehavior> Behaviors { get; }

    public BaseDbContext(DbContextOptions options, params IContextBehavior[] behaviors) : base(options)
    {
        Behaviors = behaviors;
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var contextBehavior in Behaviors)
        {
            contextBehavior.BeforeSaveChanges(ChangeTracker);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        foreach (var contextBehavior in Behaviors)
        {
            contextBehavior.BeforeSaveChanges(ChangeTracker);
        }

        return base.SaveChanges();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        for (var i = 0; i < Behaviors.Count; i++)
        {
            var contextBehavior = Behaviors[i];
            contextBehavior.AddGlobalQueryFilter(modelBuilder, this, i);
        }
    }
}