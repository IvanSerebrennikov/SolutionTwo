using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Extensions;

namespace SolutionTwo.Data.Common.Context;

public class BaseDbContext : DbContext
{
    public BaseDbContext(DbContextOptions options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletableEntity>(x => !x.IsDeleted);
    }
}