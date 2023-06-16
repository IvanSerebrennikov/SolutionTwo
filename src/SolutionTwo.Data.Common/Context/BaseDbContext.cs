using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.ContextBehaviors.Base.Interfaces;

namespace SolutionTwo.Data.Common.Context;

public class BaseDbContext : DbContext
{
    private readonly IReadOnlyList<IContextBehavior> _behaviors;

    public BaseDbContext(DbContextOptions options, params IContextBehavior[] behaviors) : base(options)
    {
        _behaviors = behaviors;
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var contextBehavior in _behaviors)
        {
            contextBehavior.BeforeSaveChanges(ChangeTracker);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        foreach (var contextBehavior in _behaviors)
        {
            contextBehavior.BeforeSaveChanges(ChangeTracker);
        }

        return base.SaveChanges();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var contextBehavior in _behaviors)
        {
            contextBehavior.OnModelCreating(modelBuilder);
        }
    }
}