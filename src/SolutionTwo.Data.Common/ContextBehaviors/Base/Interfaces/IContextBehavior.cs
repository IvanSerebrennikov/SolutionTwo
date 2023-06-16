using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SolutionTwo.Data.Common.Context;

namespace SolutionTwo.Data.Common.ContextBehaviors.Base.Interfaces;

public interface IContextBehavior
{
    void AddGlobalQueryFilter(ModelBuilder modelBuilder, BaseDbContext context, int behaviorIndex);

    void BeforeSaveChanges(ChangeTracker changeTracker);
}