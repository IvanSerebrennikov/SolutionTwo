using Microsoft.EntityFrameworkCore;
using SolutionTwo.Common.MultiTenancy.Interfaces;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.Extensions;

namespace SolutionTwo.Data.Common.Features.MultiTenancy;

public class MultiTenancyContextBehavior : IMultiTenancyContextBehavior
{
    private readonly ITenantAccessGetter _tenantAccessGetter;

    public MultiTenancyContextBehavior(ITenantAccessGetter tenantAccessGetter)
    {
        _tenantAccessGetter = tenantAccessGetter;
    }

    public void AddGlobalQueryFilter(BaseDbContext context, ModelBuilder modelBuilder)
    {
        // _tenantAccessGetter should be accessed through context.Behaviors[...] because
        // if global query expression uses some service then this service should be accessed through context
        // to make it possible for EF to translate expression into SQL
        
        modelBuilder.AppendGlobalQueryFilter<IOwnedByTenantEntity>(x =>
            (((MultiTenancyContextBehavior)context.Behaviors[typeof(MultiTenancyContextBehavior)])._tenantAccessGetter
            .AllTenantsAccessible) ||
            x.TenantId ==
            (((MultiTenancyContextBehavior)context.Behaviors[typeof(MultiTenancyContextBehavior)])._tenantAccessGetter
             .TenantId ??
             Guid.Empty));
    }

    public void BeforeSaveChanges(BaseDbContext context)
    {
        if (_tenantAccessGetter.AllTenantsAccessible)
        {
            return;
        }
        
        if (!_tenantAccessGetter.TenantId.HasValue || _tenantAccessGetter.TenantId.Value == Guid.Empty)
        {
            throw new ApplicationException("TenantAccessGetter can't provide access to any tenant");
        }

        foreach (var entry in context.ChangeTracker.Entries<IOwnedByTenantEntity>()
                     .Where(entry => entry.State == EntityState.Added))
        {
            entry.Entity.TenantId = _tenantAccessGetter.TenantId.Value;
        }
    }
}