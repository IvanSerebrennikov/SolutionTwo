using SolutionTwo.Data.Context;
using SolutionTwo.Data.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.UnitOfWork;

public class MainDatabase : IMainDatabase
{
    private readonly MainDatabaseContext _context;

    public MainDatabase(MainDatabaseContext context)
    {
        _context = context;
    }

    public async Task CommitChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void CommitChanges()
    {
        _context.SaveChanges();
    }
}