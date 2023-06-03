using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Entities.ManyToMany;

namespace SolutionTwo.Data.MainDatabase.Context;

public class MainDatabaseContext : DbContext
{
    public MainDatabaseContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasMany(x => x.Roles)
            .WithMany(x => x.Users)
            .UsingEntity<UserRoleRelation>(
                l => l.HasOne<RoleEntity>().WithMany().HasForeignKey(e => e.RoleId),
                r => r.HasOne<UserEntity>().WithMany().HasForeignKey(e => e.UserId),
                x => x.ToTable("UserRoles"));
        
        modelBuilder.Entity<UserEntity>().HasQueryFilter(x => !x.IsDeleted);
    }
}