using System.Linq.Expressions;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.MultiTenancy.Entities.Interfaces;
using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Common.MultiTenancy;
using SolutionTwo.Data.Common.MultiTenancy.Context;

namespace SolutionTwo.Data.Common.MultiTenancy.Repositories;

public abstract class BaseMultiTenancyRepository<TContext, TEntity, TId> : BaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>, IOwnedByTenantEntity
    where TContext : MultiTenancyDbContext
{
    private readonly ITenantAccessGetter _tenantAccessGetter;
    
    protected BaseMultiTenancyRepository(TContext context, ITenantAccessGetter tenantAccessGetter) : base(context)
    {
        _tenantAccessGetter = tenantAccessGetter;
    }

    protected override Expression<Func<TEntity, bool>>? GetFilterForEachQuery()
    {
        if (!_tenantAccessGetter.IsInitialized)
        {
            throw new ApplicationException("Tenant Access is not initialized");
        }
        
        if (!_tenantAccessGetter.AllTenantsAccessible && _tenantAccessGetter.TenantId.HasValue)
        {
            return x => x.TenantId == _tenantAccessGetter.TenantId.Value;
        }

        return base.GetFilterForEachQuery();
    }
}