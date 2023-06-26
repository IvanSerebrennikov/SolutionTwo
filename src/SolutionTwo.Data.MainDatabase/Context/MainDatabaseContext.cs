using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.Features.Audit;
using SolutionTwo.Data.Common.Features.MultiTenancy;
using SolutionTwo.Data.Common.Features.OptimisticConcurrency;
using SolutionTwo.Data.Common.Features.SoftDeletion;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Entities.ManyToMany;

namespace SolutionTwo.Data.MainDatabase.Context;

public class MainDatabaseContext : BaseDbContext
{
    public MainDatabaseContext(
        DbContextOptions<MainDatabaseContext> options,
        ISoftDeletionContextBehavior softDeletionContextBehavior,
        IAuditContextBehavior auditContextBehavior,
        IOptimisticConcurrencyContextBehavior optimisticConcurrencyContextBehavior,
        IMultiTenancyContextBehavior multiTenancyContextBehavior) :
        base(options,
            softDeletionContextBehavior,
            auditContextBehavior,
            optimisticConcurrencyContextBehavior,
            multiTenancyContextBehavior)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<RoleEntity> Roles => Set<RoleEntity>();

    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
    
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    
    public DbSet<ProductUsageEntity> ProductUsages => Set<ProductUsageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasMany(x => x.Roles)
            .WithMany(x => x.Users)
            .UsingEntity<UserRoleRelation>(
                l => l.HasOne<RoleEntity>().WithMany().HasForeignKey(e => e.RoleId),
                r => r.HasOne<UserEntity>().WithMany().HasForeignKey(e => e.UserId),
                x => x.ToTable("UserRoles"));

        base.OnModelCreating(modelBuilder);
    }
}