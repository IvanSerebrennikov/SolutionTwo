using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.MultiTenant.Entities.Interfaces;
using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.MultiTenancy;

namespace SolutionTwo.Data.Common.MultiTenant.Repositories;

public abstract class BaseMultiTenantRepository<TEntity, TId> : BaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>, IOwnedByTenantEntity
{
    private readonly ITenantAccessGetter _tenantAccessGetter;
    
    protected BaseMultiTenantRepository(DbContext context, ITenantAccessGetter tenantAccessGetter) : base(context)
    {
        _tenantAccessGetter = tenantAccessGetter;
    }

    protected override Expression<Func<TEntity, bool>>? CommonPredicate()
    {
        if (!_tenantAccessGetter.IsInitialized)
        {
            throw new ApplicationException("TenantAccessGetter is not initialized");
        }
        
        if (!_tenantAccessGetter.AllTenantsAccessible && _tenantAccessGetter.TenantId.HasValue)
        {
            return x => x.TenantId == _tenantAccessGetter.TenantId.Value;
        }

        return base.CommonPredicate();
    }
}