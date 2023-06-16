using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.ContextBehaviors.Base.Interfaces;

namespace SolutionTwo.Data.Common.Context;

public class BaseDbContext : DbContext
{
    public IReadOnlyDictionary<Type, IContextBehavior> Behaviors { get; }

    public BaseDbContext(DbContextOptions options, params IContextBehavior[] behaviors) : base(options)
    {
        Behaviors = behaviors.ToDictionary(x => x.GetType());
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var contextBehavior in Behaviors)
        {
            contextBehavior.Value.BeforeSaveChanges(this);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        foreach (var contextBehavior in Behaviors)
        {
            contextBehavior.Value.BeforeSaveChanges(this);
        }

        return base.SaveChanges();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var contextBehavior in Behaviors)
        {
            contextBehavior.Value.AddGlobalQueryFilter(this, modelBuilder);
        }
    }
}