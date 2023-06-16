using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Context;

namespace SolutionTwo.Data.Common.ContextBehaviors.Base.Interfaces;

public interface IContextBehavior
{
    void AddGlobalQueryFilter(BaseDbContext context, ModelBuilder modelBuilder);

    void BeforeSaveChanges(BaseDbContext context);
}