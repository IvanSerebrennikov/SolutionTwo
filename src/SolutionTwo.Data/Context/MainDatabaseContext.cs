using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Entities;

namespace SolutionTwo.Data.Context;

public class MainDatabaseContext : DbContext
{
    public MainDatabaseContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}