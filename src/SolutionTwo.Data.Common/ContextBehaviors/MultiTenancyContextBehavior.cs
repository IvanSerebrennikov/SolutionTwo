using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SolutionTwo.Common.MultiTenancy;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.ContextBehaviors.Interfaces;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Extensions;

namespace SolutionTwo.Data.Common.ContextBehaviors;

public class MultiTenancyContextBehavior : IMultiTenancyContextBehavior
{
    private readonly ITenantAccessGetter _tenantAccessGetter;

    public MultiTenancyContextBehavior(ITenantAccessGetter tenantAccessGetter)
    {
        _tenantAccessGetter = tenantAccessGetter;
    }

    public void AddGlobalQueryFilter(ModelBuilder modelBuilder, BaseDbContext context, int behaviorIndex)
    {
        modelBuilder.AppendGlobalQueryFilter<IOwnedByTenantEntity>(x =>
            ((MultiTenancyContextBehavior)context.Behaviors[behaviorIndex])._tenantAccessGetter.AllTenantsAccessible ||
            x.TenantId ==
            (((MultiTenancyContextBehavior)context.Behaviors[behaviorIndex])._tenantAccessGetter.TenantId ??
             Guid.Empty));
    }

    public void BeforeSaveChanges(ChangeTracker changeTracker)
    {
        if (_tenantAccessGetter.AllTenantsAccessible)
        {
            return;
        }
        
        if (!_tenantAccessGetter.TenantId.HasValue || _tenantAccessGetter.TenantId.Value == Guid.Empty)
        {
            throw new ApplicationException("TenantAccessGetter can't provide access to any tenant");
        }

        foreach (var entry in changeTracker.Entries<IOwnedByTenantEntity>()
                     .Where(entry => entry.State == EntityState.Added))
        {
            entry.Entity.TenantId = _tenantAccessGetter.TenantId.Value;
        }
    }
}