using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Entities.ManyToMany;

namespace SolutionTwo.Data.Context;

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
            .UsingEntity<UserRoleEntity>(
                l => l.HasOne<RoleEntity>().WithMany().HasForeignKey(e => e.RoleId),
                r => r.HasOne<UserEntity>().WithMany().HasForeignKey(e => e.UserId),
                x => x.ToTable("UserRoles"));
    }
}