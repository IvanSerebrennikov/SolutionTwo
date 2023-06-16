using Microsoft.EntityFrameworkCore;
using SolutionTwo.Common.MultiTenancy;
using SolutionTwo.Data.Common.Context;
using SolutionTwo.Data.Common.ContextBehaviors;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Entities.ManyToMany;

namespace SolutionTwo.Data.MainDatabase.Context;

public class MainDatabaseContext : BaseDbContext
{
    public MainDatabaseContext(
        DbContextOptions options, 
        ITenantAccessGetter tenantAccessGetter) : 
        base(options,
        new SoftDeletionContextBehavior(), 
        new MultiTenancyContextBehavior(tenantAccessGetter))
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

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