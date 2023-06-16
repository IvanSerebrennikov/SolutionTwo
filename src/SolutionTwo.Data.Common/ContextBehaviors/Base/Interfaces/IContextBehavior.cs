using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SolutionTwo.Data.Common.ContextBehaviors.Base.Interfaces;

public interface IContextBehavior
{
    void OnModelCreating(ModelBuilder modelBuilder);

    void BeforeSaveChanges(ChangeTracker changeTracker);
}