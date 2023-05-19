using SolutionTwo.Data.Context;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork.Interfaces;

namespace SolutionTwo.Data.UnitOfWork;

public class MainDatabase : IMainDatabase
{
    private readonly MainDatabaseContext _context;
    
    public IUserRepository Users { get; }

    public MainDatabase(MainDatabaseContext context, IUserRepository userRepository)
    {
        _context = context;
        Users = userRepository;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}