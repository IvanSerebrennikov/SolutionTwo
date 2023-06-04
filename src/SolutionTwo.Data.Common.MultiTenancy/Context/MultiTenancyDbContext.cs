using Microsoft.EntityFrameworkCore;
using SolutionTwo.Common.MultiTenancy;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.Extensions;
using SolutionTwo.Data.Common.MultiTenancy.Entities.Interfaces;

namespace SolutionTwo.Data.Common.MultiTenancy.Context;

public class MultiTenancyDbContext : BaseDbContext
{
    private readonly ITenantAccessGetter _tenantAccessGetter;
    
    public MultiTenancyDbContext(DbContextOptions options, ITenantAccessGetter tenantAccessGetter) : base(options)
    {
        _tenantAccessGetter = tenantAccessGetter;
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleMultiTenancyBeforeSaveChanges();

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        HandleMultiTenancyBeforeSaveChanges();

        return base.SaveChanges();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AppendGlobalQueryFilter<IOwnedByTenantEntity>(x =>
            !_tenantAccessGetter.TenantId.HasValue || x.TenantId == (_tenantAccessGetter.TenantId ?? Guid.Empty));
        
        base.OnModelCreating(modelBuilder);
    }

    private void HandleMultiTenancyBeforeSaveChanges()
    {
        if (!_tenantAccessGetter.IsInitialized) 
            throw new ApplicationException("Tenant Access is not initialized");

        if (!_tenantAccessGetter.TenantId.HasValue || _tenantAccessGetter.TenantId.Value == Guid.Empty)
            return;

        foreach (var entry in ChangeTracker.Entries<IOwnedByTenantEntity>()
                     .Where(entry => entry.State == EntityState.Added))
        {
            entry.Entity.TenantId = _tenantAccessGetter.TenantId.Value;
        }
    }
}